using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI의 전체 뷰(View)를 담당하는 클래스.
    /// UI 요소들에 대한 참조를 가지고 있으며, 프레젠터의 명령에 따라 화면을 갱신함.
    /// 
    /// 레이아웃:
    ///   미리보기(좌측) | 탭(우측 상단) + 나가기(우측 상단 끝)
    ///                  | 아이템 목록(우측 중단)
    ///                  | 구매 버튼(우측 하단)
    /// </summary>
    public class RcUIShopPanel : RcUIPanel
    {
        public void OpenManaged()
        {
            RcUIManager.Instance.OpenShopPanel();
        }

        [Header("Bind Widgets")]
        [SerializeField] private RcUICommonTabList tabList;
        
        [Header("Widget Class")]
        [SerializeField] private RcUIShopItem itemTemplate;
        
        [Header("Buttons")]
        [SerializeField] private RcButton buyButton;
        [SerializeField] private RcButton closeButton;
        
        [Header("Preview & Info")]
        [SerializeField] private GameObject previewRoot;
        [SerializeField] private TMPro.TMP_Text itemNameText;
        [SerializeField] private TMPro.TMP_Text priceText;

        private RcUIShopPresenter presenter;
        private readonly Dictionary<int, List<RcUIShopItem>> tabItemPools = new();

        public event Action OnCloseClicked;
        public event Action OnBuyClicked;
        
        public event Action<int> OnTabChanged
        {
            add => tabList.OnTabChanged += value;
            remove => tabList.OnTabChanged -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (closeButton != null) closeButton.OnClick += () => OnCloseClicked?.Invoke();
            if (buyButton != null) buyButton.OnClick += () => OnBuyClicked?.Invoke();
            
            if (tabList != null) tabList.Initialize();
            
            if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        }

        protected override void OnOpen()
        {
            presenter = new RcUIShopPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void SelectTab(int index)
        {
            if (tabList != null)
                tabList.SelectTab(index);
        }

        public void SetSelectedItemName(string name)
        {
            if (itemNameText != null)
                itemNameText.text = name;
        }

        public void SetSelectedItemPrice(string price)
        {
            if (priceText != null)
                priceText.text = price;
        }

        public void SetBuyButtonInteractable(bool interactable)
        {
            if (buyButton != null)
                buyButton.Interactable = interactable;
        }

        public Transform CurrentItemListParent => tabList != null ? tabList.ActiveTargetContent : null;

        public void RefreshItemList(int tabIndex, int count, Action<int, RcUIShopItem> binder)
        {
            var parent = CurrentItemListParent;
            if (parent == null) return;

            // 해당 탭 전용 격리 풀 획득
            if (!tabItemPools.TryGetValue(tabIndex, out var pool))
            {
                pool = new List<RcUIShopItem>();
                tabItemPools[tabIndex] = pool;
            }

            // 해당 탭 풀 내부의 모든 아이템 비활성화
            foreach (var item in pool) item.gameObject.SetActive(false);

            for (int i = 0; i < count; i++)
            {
                RcUIShopItem widget;
                if (i < pool.Count)
                {
                    widget = pool[i];
                }
                else
                {
                    widget = Instantiate(itemTemplate, parent);
                    widget.Initialize();
                    pool.Add(widget);
                }

                widget.gameObject.SetActive(true);
                binder?.Invoke(i, widget);
            }
        }

        /// <summary>
        /// 이미 표시 중인 슬롯들의 상태(선택/보유/잠금)만 갱신한다.
        /// Setup(아이콘·색·가격) 재호출이 없어 선택 변경 시 전체 재바인딩보다 가볍다.
        /// </summary>
        public void RefreshItemStates(int tabIndex, Action<int, RcUIShopItem> stateBinder)
        {
            if (stateBinder == null) return;
            if (!tabItemPools.TryGetValue(tabIndex, out var pool)) return;

            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf) continue;
                stateBinder(i, pool[i]);
            }
        }

        private void OnDestroy()
        {
            foreach (var pool in tabItemPools.Values)
            {
                foreach (var item in pool) item.Cleanup();
            }
            if (tabList != null) tabList.Cleanup();
        }
    }
}
