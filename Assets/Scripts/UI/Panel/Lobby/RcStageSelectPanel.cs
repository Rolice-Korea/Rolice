using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcStageSelectPanel : RcUIPanel
    {
        [Header("References")]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private RcStageItemWidget _itemTemplate;

        private readonly List<RcStageItemWidget> _items = new();
        private RcStageSelectPresenter _presenter;
        private int _selectedIndex = -1;

        public event Action<int> OnStageSelected;
        public int ItemCount => _items.Count;

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

        public void CreateItems(int count)
        {
            ClearItems();

            for (int i = 0; i < count; i++)
            {
                var item = Instantiate(_itemTemplate, _contentParent);
                item.gameObject.SetActive(true);
                item.Initialize();
                item.OnStageSelected += HandleStageSelected;
                _items.Add(item);
            }
        }

        public void SetItemData(int index, int stageNumber, RcStageState state, int stars)
        {
            if (index < 0 || index >= _items.Count) return;
            _items[index].SetData(stageNumber, state, stars);
        }

        public void SelectItem(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            if (_items[index].State == RcStageState.Locked) return;

            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                _items[_selectedIndex].SetSelected(false);

            _selectedIndex = index;
            _items[_selectedIndex].SetSelected(true);
        }

        public void ClearItems()
        {
            foreach (var item in _items)
            {
                item.OnStageSelected -= HandleStageSelected;
                item.Cleanup();
                Destroy(item.gameObject);
            }

            _items.Clear();
            _selectedIndex = -1;
        }

        private void HandleStageSelected(int stageNumber)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].StageNumber == stageNumber)
                {
                    SelectItem(i);
                    break;
                }
            }

            OnStageSelected?.Invoke(stageNumber);
        }

        private void OnDestroy()
        {
            ClearItems();
        }
    }
}
