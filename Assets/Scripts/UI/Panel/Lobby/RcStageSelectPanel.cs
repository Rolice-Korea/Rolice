using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 스테이지 선택 패널
    /// </summary>
    public class RcStageSelectPanel : RcUIPanel
    {
        [Header("References")]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private RcStageItemWidget _itemPrefab;

        private readonly List<RcStageItemWidget> _items = new();
        private RcStageSelectPresenter _presenter;

        public event Action<int> OnStageSelected;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnOpen()
        {
            _presenter = new RcStageSelectPresenter();
            _presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            _presenter?.Unbind();
            _presenter = null;
        }

        /// <summary>
        /// 스테이지 아이템 생성
        /// </summary>
        public void CreateItems(int count)
        {
            ClearItems();

            for (int i = 0; i < count; i++)
            {
                var item = Instantiate(_itemPrefab, _contentParent);
                item.Initialize();
                item.OnStageSelected += HandleStageSelected;
                _items.Add(item);
            }
        }

        /// <summary>
        /// 특정 스테이지 아이템 데이터 설정
        /// </summary>
        public void SetItemData(int index, int stageNumber, RcStageState state, int stars)
        {
            if (index < 0 || index >= _items.Count) return;

            _items[index].SetData(stageNumber, state, stars);
        }

        /// <summary>
        /// 모든 아이템 제거
        /// </summary>
        public void ClearItems()
        {
            foreach (var item in _items)
            {
                item.OnStageSelected -= HandleStageSelected;
                item.Cleanup();
                Destroy(item.gameObject);
            }
            _items.Clear();
        }

        /// <summary>
        /// 아이템 개수
        /// </summary>
        public int ItemCount => _items.Count;

        private void HandleStageSelected(int stageNumber)
        {
            OnStageSelected?.Invoke(stageNumber);
        }

        private void OnDestroy()
        {
            ClearItems();
        }
    }
}
