using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice.System;
using UnityEngine;

namespace Rolice.UI
{
    public class RcStageStartDialogPresenter : RcUIPresenter<RcStageStartDialog>
    {
        private bool _isSyncing;

        protected override void OnInitialize()
        {
            Panel.StartButton.OnClick += HandleStart;
            Panel.CancelButton.OnClick += HandleCancel;
            PopulateView(Panel.Data);
        }

        protected override void OnDispose()
        {
            Panel.StartButton.OnClick -= HandleStart;
            Panel.CancelButton.OnClick -= HandleCancel;
        }

        private void PopulateView(RcStageStartDialogData data)
        {
            Panel.SetTitle(data.StageName);
            PopulateConditions(data.MoveCountThreshold, data.TimeThreshold);
            Panel.SetProgress(data.CurrentStars);
        }

        private void PopulateConditions(int moveCountThreshold, float timeThreshold)
        {
            string timeText = timeThreshold > 0f ? $"{timeThreshold:0} SEC" : "-";
            Panel.SetConditionRow(0, 3, timeText);

            string moveText = moveCountThreshold > 0 ? $"{moveCountThreshold} MOVES" : "-";
            Panel.SetConditionRow(1, 2, moveText);

            Panel.SetConditionRow(2, 1, "CLEAR");
        }

        private void HandleStart() => HandleStartAsync().Forget();

        private async UniTaskVoid HandleStartAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            bool heartSpent = false;
            await RcSystemDialogManager.Instance.ShowUntilSuccessAsync(async () =>
            {
                heartSpent = await RcHeartManager.Instance.TrySpendAsync();
            }, "Connection failed.\nPlease retry.");

            if (!heartSpent)
            {
                Panel.Close();
                RcUIManager.Instance.Open<RcHeartShopPanel>();
                _isSyncing = false;
                return;
            }

            _isSyncing = false;
            Panel.Close();
            RcGameFlowManager.Instance.StartRandomGame(RcGameContext.SelectedGridSize);
        }

        private void HandleCancel()
        {
            Panel.Close();
        }
    }
}
