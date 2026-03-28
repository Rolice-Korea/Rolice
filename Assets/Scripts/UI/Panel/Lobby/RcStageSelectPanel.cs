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
        [SerializeField] private int poolSize = 7; // 화면에 표시할 최대 위젯 수 (홀수 권장)

        // 가상화 상태
        private RcStageItemWidget[] pool;
        private int[] widgetStageIndex; // pool[i]가 현재 표시 중인 stageIndex (-1 = 미할당)
        private int totalStageCount;
        private Func<int, StageItemData> dataProvider;

        private RcStageSelectPresenter presenter;
        private int currentIndex;
        private Sequence carouselSequence;

        // 물리 상태
        private float scrollPos;
        private float scrollVelocity;
        private bool physicsActive;

        // 드래그 상태
        private float dragLastX;
        private float dragVelocity;

        public struct StageItemData
        {
            public int StageNumber;
            public RcStageState State;
            public int Stars;
        }

        public event Action<int> OnStageSelected;
        public int ItemCount => totalStageCount;
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

        // ─────────────────────────────────────────────────
        // 가상화 API
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Pool을 재생성하지 않고 현재 표시 중인 위젯의 데이터만 갱신한다.
        /// totalCount가 변하지 않았을 때 OnProgressChanged 응답에 사용.
        /// </summary>
        public void RefreshVisibleData()
        {
            if (pool == null || dataProvider == null) return;
            for (int i = 0; i < pool.Length; i++)
            {
                int stageIdx = widgetStageIndex[i];
                if (stageIdx < 0) continue;
                var data = dataProvider(stageIdx);
                pool[i].SetData(data.StageNumber, data.State, data.Stars);
            }
        }

        public void SetupVirtualCarousel(int totalCount, Func<int, StageItemData> provider)
        {
            ClearPool();
            totalStageCount = totalCount;
            dataProvider    = provider;

            int actualSize = Mathf.Min(poolSize, totalCount);
            pool             = new RcStageItemWidget[actualSize];
            widgetStageIndex = new int[actualSize];

            for (int i = 0; i < actualSize; i++)
            {
                var item = Instantiate(itemTemplate, contentParent);
                item.gameObject.SetActive(false);
                item.Initialize();
                item.OnStageSelected += HandleItemClicked;
                pool[i]             = item;
                widgetStageIndex[i] = -1;
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
                    item.OnStageSelected -= HandleItemClicked;
                    item.Cleanup();
                    Destroy(item.gameObject);
                }
            }

            pool             = null;
            widgetStageIndex = null;
            totalStageCount  = 0;
            currentIndex     = 0;
            scrollPos        = 0f;
            dataProvider     = null;
        }

        // ─────────────────────────────────────────────────
        // 포커스 / 네비게이션
        // ─────────────────────────────────────────────────

        public void FocusIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, totalStageCount - 1));
            scrollPos    = currentIndex;
            RefreshLayout();
        }

        public void NavigateTo(int targetIndex, bool animated = true)
        {
            targetIndex   = Mathf.Clamp(targetIndex, 0, totalStageCount - 1);
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

        // ─────────────────────────────────────────────────
        // 진입 연출
        // ─────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────
        // 레이아웃 갱신 (가상화 핵심)
        // ─────────────────────────────────────────────────

        private void RefreshLayout()
        {
            if (pool == null || totalStageCount == 0) return;

            // 1. 현재 중심 기준 visible range 계산
            int center     = Mathf.RoundToInt(Mathf.Clamp(scrollPos, 0, totalStageCount - 1));
            int rangeMin   = Mathf.Max(0, center - HalfRange);
            int rangeMax   = Mathf.Min(totalStageCount - 1, center + HalfRange);

            // 2. 범위 밖 위젯 해제 (재활용 대상으로 마킹)
            for (int i = 0; i < pool.Length; i++)
            {
                int idx = widgetStageIndex[i];
                if (idx >= 0 && (idx < rangeMin || idx > rangeMax))
                    widgetStageIndex[i] = -1;
            }

            // 3. visible range 안에서 위젯 없는 stageIndex → 빈 슬롯에 할당
            for (int stageIdx = rangeMin; stageIdx <= rangeMax; stageIdx++)
            {
                if (IsStageAssigned(stageIdx)) continue;

                int slot = GetFreeSlot();
                if (slot < 0)
                {
                    Debug.LogError($"[VirtualCarousel] Pool 슬롯 부족 — stageIdx={stageIdx}, poolSize={pool.Length}");
                    break;
                }

                widgetStageIndex[slot] = stageIdx;
                var data = dataProvider(stageIdx);
                pool[slot].SetData(data.StageNumber, data.State, data.Stars);
            }

            // 4. 각 위젯 위치·비주얼 적용
            for (int i = 0; i < pool.Length; i++)
            {
                int stageIdx = widgetStageIndex[i];
                if (stageIdx < 0)
                {
                    pool[i].gameObject.SetActive(false);
                    continue;
                }
                pool[i].gameObject.SetActive(true);
                ApplyItemVisual(pool[i], stageIdx, scrollPos);
            }
        }

        private void ApplyItemVisual(RcStageItemWidget widget, int stageIdx, float centerPos)
        {
            float dist = Mathf.Abs(stageIdx - centerPos);
            var rt     = widget.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((stageIdx - centerPos) * itemSpacing, 0f);

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

        // ─────────────────────────────────────────────────
        // 스냅
        // ─────────────────────────────────────────────────

        private void SnapToIndex(int index)
        {
            index        = Mathf.Clamp(index, 0, totalStageCount - 1);
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
                    FindWidgetForStage(index)?.PlaySelectAnimation();
                });
            carouselSequence.Play();
        }

        private void AppendReveal(int stageIndex, float delay)
        {
            if (stageIndex < 0 || stageIndex >= totalStageCount) return;

            var widget = FindWidgetForStage(stageIndex);
            if (widget == null) return;

            float targetT = Mathf.Clamp01(1f - Mathf.Abs(stageIndex - currentIndex));
            var rt        = widget.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2((stageIndex - currentIndex) * itemSpacing, 0f);
            widget.SetSelected(stageIndex == currentIndex);

            carouselSequence.Insert(delay,
                DOTween.To(() => 0f, v => widget.EvaluateCarousel(v), targetT, 0.22f)
                       .SetEase(Ease.OutBack));
        }

        // ─────────────────────────────────────────────────
        // 드래그 / 휠 / 물리
        // ─────────────────────────────────────────────────

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
            float dx      = eventData.position.x - dragLastX;
            dragLastX     = eventData.position.x;

            float cardDelta = -dx / itemSpacing;
            scrollPos = Mathf.Clamp(scrollPos + cardDelta, 0f, totalStageCount - 1);

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
            if (!physicsActive || totalStageCount == 0) return;

            scrollPos += scrollVelocity * Time.deltaTime;

            float maxPos = totalStageCount - 1;
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

        // ─────────────────────────────────────────────────
        // 클릭 처리
        // ─────────────────────────────────────────────────

        private void HandleItemClicked(int stageNumber)
        {
            int clickedIndex = stageNumber - 1;
            if (clickedIndex < 0 || clickedIndex >= totalStageCount) return;

            if (clickedIndex != currentIndex)
                NavigateTo(clickedIndex);
            else
                OnStageSelected?.Invoke(stageNumber);
        }

        // ─────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────

        private bool IsStageAssigned(int stageIdx)
        {
            for (int i = 0; i < widgetStageIndex.Length; i++)
                if (widgetStageIndex[i] == stageIdx) return true;
            return false;
        }

        private int GetFreeSlot()
        {
            for (int i = 0; i < widgetStageIndex.Length; i++)
                if (widgetStageIndex[i] < 0) return i;
            return -1;
        }

        private RcStageItemWidget FindWidgetForStage(int stageIdx)
        {
            for (int i = 0; i < widgetStageIndex.Length; i++)
                if (widgetStageIndex[i] == stageIdx) return pool[i];
            return null;
        }
    }
}
