using System;
using System.Collections.Generic;
using DG.Tweening;
using Engine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcStageSelectPanel : RcUIPanel, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private RcStageItemWidget itemTemplate;

        [Header("Carousel — Layout")]
        [SerializeField] private float itemSpacing = 220f;

        [Header("Carousel — Alpha (dist > 1 페이드)")]
        [SerializeField] private float neighborAlpha = 0.40f;

        [Header("Carousel — Snap")]
        [SerializeField] private float snapDuration = 0.22f;

        [Header("Carousel — Scroll Physics")]
        [SerializeField] private float scrollWheelSensitivity = 3.5f;
        [SerializeField] private float scrollFriction = 7f;
        [SerializeField] private float snapVelocityThreshold = 0.4f;

        [Header("Virtualization")]
        [SerializeField] private int poolSize = 7;

        private RcStageItemWidget[] pool;
        private int[] widgetItemIndex;
        private int totalItemCount;
        private Func<int, StageItemData> dataProvider;

        private RcStageSelectPresenter presenter;
        private int currentIndex;
        private Sequence carouselSequence;

        private float scrollPos;
        private float scrollVelocity;
        private bool physicsActive;

        private float dragLastX;
        private float dragVelocity;

        public struct StageItemData
        {
            public int GridSize;
            public string Label;
            public RcStageState State;
        }

        public event Action<int> OnGridSizeSelected;
        public int ItemCount => totalItemCount;
        public int CurrentIndex => currentIndex;

        private int HalfRange => poolSize / 2;

        protected override void Awake()
        {
            base.Awake();
            if (contentParent == null) return;

            var lg = contentParent.GetComponent<LayoutGroup>();
            if (lg != null) DestroyImmediate(lg);

            var csf = contentParent.GetComponent<ContentSizeFitter>();
            if (csf != null) DestroyImmediate(csf);
        }

        protected override void OnOpen()
        {
            presenter = new RcStageSelectPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            carouselSequence?.Kill();
            physicsActive = false;
            presenter?.Unbind();
            presenter = null;
        }

        private void Update()
        {
            if (!IsOpen) return;
            HandleScrollWheel();
            UpdatePhysics();
        }

        private void OnDestroy() => ClearPool();

        public void RefreshVisibleData()
        {
            if (pool == null || dataProvider == null) return;
            for (int i = 0; i < pool.Length; i++)
            {
                int itemIdx = widgetItemIndex[i];
                if (itemIdx < 0) continue;
                var data = dataProvider(itemIdx);
                pool[i].SetData(itemIdx, data.Label, data.State);
            }
        }

        public void SetupVirtualCarousel(int totalCount, Func<int, StageItemData> provider)
        {
            ClearPool();
            totalItemCount = totalCount;
            dataProvider   = provider;

            int actualSize  = Mathf.Min(poolSize, totalCount);
            pool            = new RcStageItemWidget[actualSize];
            widgetItemIndex = new int[actualSize];

            for (int i = 0; i < actualSize; i++)
            {
                var item = Instantiate(itemTemplate, contentParent);
                item.gameObject.SetActive(false);
                item.Initialize();
                item.OnItemSelected += HandleItemClicked;
                pool[i]            = item;
                widgetItemIndex[i] = -1;
            }
        }

        public void ClearPool()
        {
            carouselSequence?.Kill();
            physicsActive = false;

            if (pool != null)
            {
                foreach (var item in pool)
                {
                    if (item == null) continue;
                    item.OnItemSelected -= HandleItemClicked;
                    item.Cleanup();
                    Destroy(item.gameObject);
                }
            }

            pool            = null;
            widgetItemIndex = null;
            totalItemCount  = 0;
            currentIndex    = 0;
            scrollPos       = 0f;
            dataProvider    = null;
        }

        public void FocusIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, totalItemCount - 1));
            scrollPos    = currentIndex;
            RefreshLayout();
        }

        public void NavigateTo(int targetIndex, bool animated = true)
        {
            targetIndex    = Mathf.Clamp(targetIndex, 0, totalItemCount - 1);
            scrollVelocity = 0f;
            physicsActive  = false;
            currentIndex   = targetIndex;

            if (animated)
                SnapToIndex(targetIndex);
            else
            {
                scrollPos = targetIndex;
                RefreshLayout();
            }
        }

        public void PlayEntryAnimation()
        {
            carouselSequence?.Kill();
            physicsActive = false;

            if (pool == null) return;
            foreach (var w in pool)
                w.EvaluateCarousel(0f);

            carouselSequence = DOTween.Sequence();

            float staggerStep = 0.07f;
            for (int dist = 0; dist <= 1; dist++)
            {
                float delay = dist * staggerStep;
                if (dist == 0)
                    AppendReveal(currentIndex, delay);
                else
                {
                    AppendReveal(currentIndex - dist, delay);
                    AppendReveal(currentIndex + dist, delay);
                }
            }

            carouselSequence.Play();
        }

        private void RefreshLayout()
        {
            if (pool == null || totalItemCount == 0) return;

            int center   = Mathf.RoundToInt(Mathf.Clamp(scrollPos, 0, totalItemCount - 1));
            int rangeMin = Mathf.Max(0, center - HalfRange);
            int rangeMax = Mathf.Min(totalItemCount - 1, center + HalfRange);

            for (int i = 0; i < pool.Length; i++)
            {
                int idx = widgetItemIndex[i];
                if (idx >= 0 && (idx < rangeMin || idx > rangeMax))
                    widgetItemIndex[i] = -1;
            }

            for (int itemIdx = rangeMin; itemIdx <= rangeMax; itemIdx++)
            {
                if (IsItemAssigned(itemIdx)) continue;

                int slot = GetFreeSlot();
                if (slot < 0)
                {
                    Debug.LogError($"[VirtualCarousel] Pool 슬롯 부족 — itemIdx={itemIdx}, poolSize={pool.Length}");
                    break;
                }

                widgetItemIndex[slot] = itemIdx;
                var data = dataProvider(itemIdx);
                pool[slot].SetData(itemIdx, data.Label, data.State);
            }

            for (int i = 0; i < pool.Length; i++)
            {
                int itemIdx = widgetItemIndex[i];
                if (itemIdx < 0)
                {
                    pool[i].gameObject.SetActive(false);
                    continue;
                }
                pool[i].gameObject.SetActive(true);
                ApplyItemVisual(pool[i], itemIdx, scrollPos);
            }
        }

        private void ApplyItemVisual(RcStageItemWidget widget, int itemIdx, float centerPos)
        {
            float dist = Mathf.Abs(itemIdx - centerPos);
            var rt     = widget.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((itemIdx - centerPos) * itemSpacing, 0f);

            widget.EvaluateCarousel(Mathf.Clamp01(1f - dist));
            widget.SetCarouselAlpha(GetAlphaForDist(dist));
            widget.SetSelected(dist < 0.5f);
        }

        private float GetAlphaForDist(float dist)
        {
            if (dist <= 1f) return Mathf.Lerp(1f, neighborAlpha, dist);
            if (dist <= 2f) return Mathf.Lerp(neighborAlpha, 0f, dist - 1f);
            return 0f;
        }

        private void SnapToIndex(int index)
        {
            index        = Mathf.Clamp(index, 0, totalItemCount - 1);
            currentIndex = index;

            carouselSequence?.Kill();
            carouselSequence = DOTween.Sequence();
            carouselSequence
                .Append(DOTween.To(
                    () => scrollPos,
                    v  => { scrollPos = v; RefreshLayout(); },
                    (float)index, snapDuration).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                    scrollPos = index;
                    FindWidgetForItem(index)?.PlaySelectAnimation();
                });
            carouselSequence.Play();
        }

        private void AppendReveal(int itemIndex, float delay)
        {
            if (itemIndex < 0 || itemIndex >= totalItemCount) return;

            var widget = FindWidgetForItem(itemIndex);
            if (widget == null) return;

            float targetT = Mathf.Clamp01(1f - Mathf.Abs(itemIndex - currentIndex));
            var rt        = widget.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((itemIndex - currentIndex) * itemSpacing, 0f);
            widget.SetSelected(itemIndex == currentIndex);

            carouselSequence.Insert(delay,
                DOTween.To(() => 0f, v => widget.EvaluateCarousel(v), targetT, 0.22f)
                       .SetEase(Ease.OutBack));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            carouselSequence?.Kill();
            physicsActive  = false;
            scrollVelocity = 0f;
            dragLastX      = eventData.position.x;
            dragVelocity   = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float dx  = eventData.position.x - dragLastX;
            dragLastX = eventData.position.x;

            float cardDelta = -dx / itemSpacing;
            scrollPos = Mathf.Clamp(scrollPos + cardDelta, 0f, totalItemCount - 1);

            float rawVel    = cardDelta / Time.deltaTime;
            bool dirFlipped = dragVelocity != 0f &&
                              Mathf.Sign(rawVel) != Mathf.Sign(dragVelocity);
            dragVelocity = dirFlipped
                ? rawVel
                : Mathf.Lerp(dragVelocity, rawVel, 0.4f);

            RefreshLayout();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            scrollVelocity = dragVelocity * 0.6f;
            physicsActive  = true;
        }

        private void HandleScrollWheel()
        {
            float wheel = Input.mouseScrollDelta.y;
            if (wheel == 0f) return;

            carouselSequence?.Kill();

            if (!physicsActive)
                scrollPos = currentIndex;

            scrollVelocity -= wheel * scrollWheelSensitivity;
            physicsActive   = true;
        }

        private void UpdatePhysics()
        {
            if (!physicsActive || totalItemCount == 0) return;

            scrollPos += scrollVelocity * Time.deltaTime;

            float maxPos = totalItemCount - 1;
            if (scrollPos <= 0f)     { scrollPos = 0f;     scrollVelocity = 0f; }
            if (scrollPos >= maxPos) { scrollPos = maxPos; scrollVelocity = 0f; }

            scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, scrollFriction * Time.deltaTime);

            RefreshLayout();

            if (Mathf.Abs(scrollVelocity) < snapVelocityThreshold)
            {
                scrollVelocity = 0f;
                physicsActive  = false;
                int target     = Mathf.RoundToInt(Mathf.Clamp(scrollPos, 0f, maxPos));
                SnapToIndex(target);
            }
        }

        private void HandleItemClicked(int clickedIndex)
        {
            if (clickedIndex < 0 || clickedIndex >= totalItemCount || dataProvider == null) return;

            if (clickedIndex != currentIndex)
                NavigateTo(clickedIndex);
            else
            {
                var data = dataProvider(clickedIndex);
                OnGridSizeSelected?.Invoke(data.GridSize);
            }
        }

        private bool IsItemAssigned(int itemIdx)
        {
            for (int i = 0; i < widgetItemIndex.Length; i++)
                if (widgetItemIndex[i] == itemIdx) return true;
            return false;
        }

        private int GetFreeSlot()
        {
            for (int i = 0; i < widgetItemIndex.Length; i++)
                if (widgetItemIndex[i] < 0) return i;
            return -1;
        }

        private RcStageItemWidget FindWidgetForItem(int itemIdx)
        {
            for (int i = 0; i < widgetItemIndex.Length; i++)
                if (widgetItemIndex[i] == itemIdx) return pool[i];
            return null;
        }
    }
}
