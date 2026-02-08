using System;

namespace Rolice.Data
{
    /// <summary>
    /// 개별 스테이지의 플레이어 진행 상황
    /// </summary>
    [Serializable]
    public class RcStageProgress
    {
        public int StageId;
        public bool IsCleared;
        public int Stars;           // 0~3
        public int BestTurnCount;   // 최고 기록 (낮을수록 좋음)

        public RcStageProgress() { }

        public RcStageProgress(int stageId)
        {
            StageId = stageId;
            IsCleared = false;
            Stars = 0;
            BestTurnCount = int.MaxValue;
        }

        /// <summary>
        /// 클리어 결과 업데이트. 기존 기록보다 좋으면 갱신
        /// </summary>
        public void UpdateClear(int turnCount, int earnedStars)
        {
            IsCleared = true;

            if (turnCount < BestTurnCount)
            {
                BestTurnCount = turnCount;
            }

            if (earnedStars > Stars)
            {
                Stars = earnedStars;
            }
        }
    }
}
