using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 스테이지 선택 프레젠터
    /// </summary>
    public class RcStageSelectPresenter : RcUIPresenter<RcStageSelectPanel>
    {
        protected override void OnInitialize()
        {
            Panel.OnStageSelected += HandleStageSelected;
            RefreshStageList();
        }

        protected override void OnDispose()
        {
            Panel.OnStageSelected -= HandleStageSelected;
        }

        /// <summary>
        /// 스테이지 목록 갱신
        /// </summary>
        public void RefreshStageList()
        {
            if (!RcProgressManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[StageSelectPresenter] ProgressManager가 초기화되지 않았습니다");
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
            Debug.Log($"[StageSelectPresenter] 스테이지 {stageNumber} 선택됨");

            // TODO: 게임씬으로 전환
            // SceneManager.LoadScene("GameScene");
            // 또는 이벤트 발행
        }
    }
}
