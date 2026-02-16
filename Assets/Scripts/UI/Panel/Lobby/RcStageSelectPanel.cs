using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcStageSelectPanel : RcUIPanel
    {
        [Header("References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private RcStageItemWidget itemTemplate;

        private readonly List<RcStageItemWidget> items = new();
        private RcStageSelectPresenter presenter;
        private int selectedIndex = -1;

        public event Action<int> OnStageSelected;
        public int ItemCount => items.Count;

        protected override void OnOpen()
        {
            presenter = new RcStageSelectPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void CreateItems(int count)
        {
            ClearItems();

            for (int i = 0; i < count; i++)
            {
                var item = Instantiate(itemTemplate, contentParent);
                item.gameObject.SetActive(true);
                item.Initialize();
                item.OnStageSelected += HandleStageSelected;
                items.Add(item);
            }
        }

        public void SetItemData(int index, int stageNumber, RcStageState state, int stars)
        {
            if (index < 0 || index >= items.Count) return;
            items[index].SetData(stageNumber, state, stars);
        }

        public void SelectItem(int index)
        {
            if (index < 0 || index >= items.Count) return;
            if (items[index].State == RcStageState.Locked) return;

            if (selectedIndex >= 0 && selectedIndex < items.Count)
                items[selectedIndex].SetSelected(false);

            selectedIndex = index;
            items[selectedIndex].SetSelected(true);
        }

        public void ClearItems()
        {
            foreach (var item in items)
            {
                item.OnStageSelected -= HandleStageSelected;
                item.Cleanup();
                Destroy(item.gameObject);
            }

            items.Clear();
            selectedIndex = -1;
        }

        private void HandleStageSelected(int stageNumber)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].StageNumber == stageNumber)
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
