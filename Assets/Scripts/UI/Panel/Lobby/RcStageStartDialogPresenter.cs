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
            PopulateConditions(data.StarThresholds);
            Panel.SetProgress(data.CurrentStars);
        }

        private void PopulateConditions(int[] thresholds)
        {
            if (thresholds == null) return;
            for (int i = 0; i < thresholds.Length; i++)
            {
                int starCount = thresholds.Length - i;
                Panel.SetConditionRow(i, starCount, thresholds[i]);
            }
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
