using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 가방 UI의 전체 뷰(View)를 담당하는 클래스.
    /// UI 요소들에 대한 참조를 가지고 있으며, 프레젠터의 명령에 따라 화면을 갱신함.
    /// </summary>
    public class RcUIBagPanel : RcUIPanel
    {
        public void OpenManaged()
        {
            RcUIManager.Instance.OpenBagPanel();
        }

        [Header("Bind Widgets")]
        [SerializeField] private RcUICommonTabList tabList;
        
        [Header("Widget Class")]
        [SerializeField] private RcUIBagItem itemTemplate;
        
        [Header("Buttons")]
        [SerializeField] private RcButton applyButton;
        [SerializeField] private RcButton resetButton;
        [SerializeField] private RcButton closeButton;
        
        [Header("Preview & Info")]
        [SerializeField] private GameObject previewRoot;
        [SerializeField] private TMPro.TMP_Text skinNameText;

        private RcUIBagPresenter presenter;
        private readonly Dictionary<int, List<RcUIBagItem>> tabItemPools = new();

        public event Action OnCloseClicked;
        public event Action OnApplyClicked;
        public event Action OnResetClicked;
        
        public event Action<int> OnTabChanged
        {
            add => tabList.OnTabChanged += value;
            remove => tabList.OnTabChanged -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (closeButton != null) closeButton.OnClick += () => OnCloseClicked?.Invoke();
            if (applyButton != null) applyButton.OnClick += () => OnApplyClicked?.Invoke();
            if (resetButton != null) resetButton.OnClick += () => OnResetClicked?.Invoke();
            
            if (tabList != null) tabList.Initialize();
            
            if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        }

        protected override void OnOpen()
        {
            presenter = new RcUIBagPresenter();
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

        public void SetSelectedSkinName(string name)
        {
            if (skinNameText != null)
                skinNameText.text = name;
        }

        public Transform CurrentItemListParent => tabList != null ? tabList.ActiveTargetContent : null;

        public void RefreshItemList(int tabIndex, int count, Action<int, RcUIBagItem> binder)
        {
            var parent = CurrentItemListParent;
            if (parent == null) return;

            // 해당 탭 전용 격리 풀 획득
            if (!tabItemPools.TryGetValue(tabIndex, out var pool))
            {
                pool = new List<RcUIBagItem>();
                tabItemPools[tabIndex] = pool;
            }

            // 해당 탭 풀 내부의 모든 아이템 비활성화
            foreach (var item in pool) item.gameObject.SetActive(false);

            for (int i = 0; i < count; i++)
            {
                RcUIBagItem widget;
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
