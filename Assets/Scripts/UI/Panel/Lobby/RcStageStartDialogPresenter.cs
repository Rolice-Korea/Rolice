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
            string timeText = timeThreshold > 0f ? $"{timeThreshold:0} SEC" : "-";
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

            // 최초 접속 이후에는 RcPlayerState가 기준 데이터.
            // 여기서 클라우드를 다시 받으면 로컬 변경(가방 스킨 등)이 서버 값으로 덮어써지므로 재동기화하지 않는다.
            int stageNumber = Panel.Data.StageNumber;

            if (!RcProgressManager.Instance.IsStageUnlocked(stageNumber))
            {
                Debug.LogWarning($"[StageStartDialog] 스테이지 {stageNumber} 잠금 상태");
                Panel.Close();
                _isSyncing = false;
                return;
            }

            // 하트 소모 시도 (네트워크 오류 시 재시도, 잔액 부족 시 하트샵 진입)
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
            RcGameFlowManager.Instance.GoToStage(stageNumber);
        }

        private void HandleCancel()
        {
            Panel.Close();
        }
    }
}
