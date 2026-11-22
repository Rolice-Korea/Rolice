using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice.System;

namespace Rolice.UI
{
    public class RcStageSelectPresenter : RcUIPresenter<RcStageSelectPanel>
    {
        private static readonly int[] GridSizes = { 4, 5, 6 };

        private bool isStarting;

        protected override void OnInitialize()
        {
            Panel.OnGridSizeSelected += HandleGridSizeSelected;
            RefreshGridSizeList();
        }

        protected override void OnDispose()
        {
            Panel.OnGridSizeSelected -= HandleGridSizeSelected;
        }

        public void RefreshGridSizeList()
        {
            if (GridSizes.Length == Panel.ItemCount)
            {
                Panel.RefreshVisibleData();
                return;
            }

            Panel.SetupVirtualCarousel(GridSizes.Length, GetGridSizeItemData);
            Panel.FocusIndex(GetInitialFocusIndex());
            Panel.PlayEntryAnimation();
        }

        private RcStageSelectPanel.StageItemData GetGridSizeItemData(int index)
        {
            int size = GridSizes[index];
            return new RcStageSelectPanel.StageItemData
            {
                GridSize = size,
                Label    = $"{size}x{size}",
                State    = RcStageState.Unlocked,
            };
        }

        private int GetInitialFocusIndex()
        {
            int currentSize = RcGameContext.SelectedGridSize;
            for (int i = 0; i < GridSizes.Length; i++)
            {
                if (GridSizes[i] == currentSize)
                    return i;
            }
            return 0;
        }

        private void HandleGridSizeSelected(int gridSize)
        {
            StartGameAsync(gridSize).Forget();
        }

        private async UniTaskVoid StartGameAsync(int gridSize)
        {
            if (isStarting) return;
            isStarting = true;

            bool heartSpent = false;
            await RcSystemDialogManager.Instance.ShowUntilSuccessAsync(async () =>
            {
                heartSpent = await RcHeartManager.Instance.TrySpendAsync();
            }, "Connection failed.\nPlease retry.");

            if (!heartSpent)
            {
                RcUIManager.Instance.Open<RcHeartShopPanel>();
                isStarting = false;
                return;
            }

            isStarting = false;
            RcGameFlowManager.Instance.StartRandomGame(gridSize);
        }
    }
}
