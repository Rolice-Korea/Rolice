using Engine.UI;
using Rolice.System;
using UnityEngine;

namespace Rolice.UI
{
    public class RcStageSelectPresenter : RcUIPresenter<RcStageSelectPanel>
    {

        protected override void OnInitialize()
        {
            Panel.OnStageSelected += HandleStageSelected;
            RcPlayerState.Instance.OnProgressChanged += RefreshStageList;
            RefreshStageList();
        }

        protected override void OnDispose()
        {
            Panel.OnStageSelected -= HandleStageSelected;
            RcPlayerState.Instance.OnProgressChanged -= RefreshStageList;
        }

        public void RefreshStageList()
        {
            if (!RcProgressManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[StageSelectPresenter] ProgressManager 미초기화");
                return;
            }

            int totalStages = RcProgressManager.Instance.TotalStageCount;
            Panel.CreateItems(totalStages);

            for (int i = 0; i < totalStages; i++)
            {
                int stageNumber = i + 1;
                var state = GetStageState(stageNumber);
                int stars = RcProgressManager.Instance.GetStageStars(stageNumber);
                Panel.SetItemData(i, stageNumber, state, stars);
            }

            Panel.FocusIndex(GetInitialFocusIndex());
            Panel.PlayEntryAnimation();
        }

        private int GetInitialFocusIndex()
        {
            int total = RcProgressManager.Instance.TotalStageCount;
            for (int i = 0; i < total; i++)
            {
                if (GetStageState(i + 1) != RcStageState.Cleared)
                    return i;
            }
            return total - 1; // 전부 클리어 → 마지막 스테이지
        }

        private RcStageState GetStageState(int stageNumber)
        {
            if (!RcProgressManager.Instance.IsStageUnlocked(stageNumber))
                return RcStageState.Locked;

            if (RcProgressManager.Instance.IsStageCleared(stageNumber))
                return RcStageState.Cleared;

            return RcStageState.Unlocked;
        }

        private void HandleStageSelected(int stageNumber)
        {
            var levelData = RcProgressManager.Instance.StageDatabase.GetStage(stageNumber);
            if (levelData == null)
            {
                Debug.LogError($"[StageSelectPresenter] 스테이지 {stageNumber} 데이터 없음");
                return;
            }

            var stageInfo = levelData.StageInfo;
            int currentStars = RcProgressManager.Instance.GetStageStars(stageNumber);

            RcUIManager.Instance.Open<RcStageStartDialog, RcStageStartDialogData>(new RcStageStartDialogData
            {
                StageNumber = stageNumber,
                StageName = stageInfo.GetDisplayName(),
                StarThresholds = stageInfo.StarThresholds,
                CurrentStars = currentStars,
            });
        }
    }
}
