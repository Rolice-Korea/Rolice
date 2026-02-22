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
        [SerializeField] private float scrollWheelSensitivity = 3.5f;  // 휠 1틱 당 속도(카드/s)
        [SerializeField] private float scrollFriction = 7f;             // 감속 계수
        [SerializeField] private float snapVelocityThreshold = 0.4f;   // 이 속도 이하 → 스냅

        private readonly List<RcStageItemWidget> items = new();
        private RcStageSelectPresenter presenter;
        private int currentIndex;
        private Sequence carouselSequence;

        // 물리 상태
        private float scrollPos;      // 연속 위치 (카드 단위, 정수 = 스냅된 상태)
        private float scrollVelocity; // 카드/초
        private bool physicsActive;

        // 드래그 상태
        private float dragLastX;
        private float dragVelocity;

        public event Action<int> OnStageSelected;
        public int ItemCount => items.Count;
        public int CurrentIndex => currentIndex;

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

        private void OnDestroy() => ClearItems();

        public void CreateItems(int count)
        {
            ClearItems();
            for (int i = 0; i < count; i++)
            {
                var item = Instantiate(itemTemplate, contentParent);
                item.gameObject.SetActive(true);
                item.Initialize();
                item.OnStageSelected += HandleItemClicked;
                items.Add(item);
            }
        }

        public void SetItemData(int index, int stageNumber, RcStageState state, int stars)
        {
            if (index < 0 || index >= items.Count) return;
            items[index].SetData(stageNumber, state, stars);
        }

        public void FocusIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, items.Count - 1));
            scrollPos = currentIndex;
            RefreshLayout();
        }

        public void PlayEntryAnimation()
        {
            carouselSequence?.Kill();
            physicsActive = false;

            foreach (var t in items)
                t.EvaluateCarousel(0f);

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

        public void ClearItems()
        {
            carouselSequence?.Kill();
            physicsActive = false;
            foreach (var item in items)
            {
                item.OnStageSelected -= HandleItemClicked;
                item.Cleanup();
                Destroy(item.gameObject);
            }
            items.Clear();
            currentIndex = 0;
            scrollPos = 0f;
        }

        public void NavigateTo(int targetIndex, bool animated = true)
        {
            targetIndex = Mathf.Clamp(targetIndex, 0, items.Count - 1);
            scrollVelocity = 0f;
            physicsActive = false;
            currentIndex = targetIndex;

            if (animated)
                SnapToIndex(targetIndex);
            else
            {
                scrollPos = targetIndex;
                RefreshLayout();
            }
        }

        private void RefreshLayout()
        {
            for (int i = 0; i < items.Count; i++)
                ApplyItemVisual(i, scrollPos);
        }

        private void ApplyItemVisual(int i, float centerPos)
        {
            float dist = Mathf.Abs(i - centerPos);
            var rt = items[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((i - centerPos) * itemSpacing, 0f);

            items[i].EvaluateCarousel(Mathf.Clamp01(1f - dist));
            items[i].SetCarouselAlpha(GetAlphaForDist(dist));

            items[i].SetSelected(dist < 0.5f);
        }

        private float GetAlphaForDist(float dist)
        {
            if (dist <= 1f) return Mathf.Lerp(1f, neighborAlpha, dist);
            if (dist <= 2f) return Mathf.Lerp(neighborAlpha, 0f, dist - 1f);
            return 0f;
        }

        private void SnapToIndex(int index)
        {
            index = Mathf.Clamp(index, 0, items.Count - 1);
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
                    items[index].PlaySelectAnimation();
                });
            carouselSequence.Play();
        }

        private void AppendReveal(int index, float delay)
        {
            if (index < 0 || index >= items.Count) return;

            float targetT = Mathf.Clamp01(1f - Mathf.Abs(index - currentIndex));

            var rt = items[index].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((index - currentIndex) * itemSpacing, 0f);
            items[index].SetSelected(index == currentIndex);

            carouselSequence.Insert(delay,
                DOTween.To(() => 0f, v => items[index].EvaluateCarousel(v), targetT, 0.22f)
                       .SetEase(Ease.OutBack));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            carouselSequence?.Kill();
            physicsActive = false;
            scrollVelocity = 0f;
            dragLastX    = eventData.position.x;
            dragVelocity = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float dx = eventData.position.x - dragLastX;
            dragLastX = eventData.position.x;

            float cardDelta = -dx / itemSpacing;
            scrollPos = Mathf.Clamp(scrollPos + cardDelta, 0f, items.Count - 1);

            // 방향 전환 시 즉시 갱신, 같은 방향이면 평활화
            float rawVel = cardDelta / Time.deltaTime;
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

            // 물리가 꺼져있었으면 현재 인덱스에서 시작
            if (!physicsActive)
                scrollPos = currentIndex;

            scrollVelocity -= wheel * scrollWheelSensitivity;
            physicsActive = true;
        }

        private void UpdatePhysics()
        {
            if (!physicsActive) return;

            scrollPos += scrollVelocity * Time.deltaTime;

            float maxPos = items.Count - 1;
            if (scrollPos <= 0f)     { scrollPos = 0f;     scrollVelocity = 0f; }
            if (scrollPos >= maxPos) { scrollPos = maxPos; scrollVelocity = 0f; }

            scrollVelocity = Mathf.Lerp(scrollVelocity, 0f, scrollFriction * Time.deltaTime);

            RefreshLayout();

            if (Mathf.Abs(scrollVelocity) < snapVelocityThreshold)
            {
                scrollVelocity = 0f;
                physicsActive = false;
                int target = Mathf.RoundToInt(Mathf.Clamp(scrollPos, 0f, maxPos));
                SnapToIndex(target);
            }
        }

        private void HandleItemClicked(int stageNumber)
        {
            int clickedIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].StageNumber == stageNumber)
                {
                    clickedIndex = i;
                    break;
                }
            }

            if (clickedIndex < 0) return;

            if (clickedIndex != currentIndex)
                NavigateTo(clickedIndex);
            else
                OnStageSelected?.Invoke(stageNumber);
        }
    }
}
