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
            // Row 0 (★★★): 시간 조건
            string timeText = timeThreshold > 0f ? $"{timeThreshold:0}SEC" : "-";
            Panel.SetConditionRow(0, 3, timeText);

            // Row 1 (★★☆): 횟수 조건
            string moveText = moveCountThreshold > 0 ? $"{moveCountThreshold} MOVES" : "-";
            Panel.SetConditionRow(1, 2, moveText);

            // Row 2 (★☆☆): 클리어
            Panel.SetConditionRow(2, 1, "CLEAR");
        }

        private void HandleStart() => HandleStartAsync().Forget();

        private async UniTaskVoid HandleStartAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            await RcPlayerState.Instance.SyncFromCloudAsync();

            int stageNumber = Panel.Data.StageNumber;

            if (!RcProgressManager.Instance.IsStageUnlocked(stageNumber))
            {
                Debug.LogWarning($"[StageStartDialog] 스테이지 {stageNumber} 잠금 상태 (클라우드 재검증)");
                Panel.Close();
                _isSyncing = false;
                return;
            }

            _isSyncing = false;
            Panel.Close();
            RcGameFlowManager.Instance.GoToStage(stageNumber);
        }

        private void HandleCancel()
        {
            Panel.Close();
        }
    }
}
