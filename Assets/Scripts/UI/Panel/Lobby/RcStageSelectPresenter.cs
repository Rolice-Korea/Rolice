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

            // 스테이지 수가 같으면 pool 재생성 없이 visible 위젯 데이터만 갱신
            if (totalStages == Panel.ItemCount)
            {
                Panel.RefreshVisibleData();
                return;
            }

            Panel.SetupVirtualCarousel(totalStages, GetStageItemData);
            Panel.FocusIndex(GetInitialFocusIndex());
            Panel.PlayEntryAnimation();
        }

        private RcStageSelectPanel.StageItemData GetStageItemData(int index)
        {
            int stageNumber = index + 1;
            return new RcStageSelectPanel.StageItemData
            {
                StageNumber = stageNumber,
                State       = GetStageState(stageNumber),
                Stars       = RcProgressManager.Instance.GetStageStars(stageNumber),
            };
        }

        private int GetInitialFocusIndex()
        {
            int total = RcProgressManager.Instance.TotalStageCount;
            for (int i = 0; i < total; i++)
            {
                if (GetStageState(i + 1) != RcStageState.Cleared)
                    return i;
            }
            return total - 1;
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

            var stageInfo    = levelData.StageInfo;
            int currentStars = RcProgressManager.Instance.GetStageStars(stageNumber);

            RcUIManager.Instance.Open<RcStageStartDialog, RcStageStartDialogData>(new RcStageStartDialogData
            {
                StageNumber        = stageNumber,
                StageName          = stageInfo.GetDisplayName(),
                MoveCountThreshold = stageInfo.MoveCountThreshold,
                TimeThreshold      = stageInfo.TimeThreshold,
                CurrentStars       = currentStars,
            });
        }
    }
}
