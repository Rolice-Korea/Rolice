using System;

namespace Rolice.Data
{
    [Serializable]
    public class RcStageProgress
    {
        public int StageId;
        public bool IsCleared;
        public int Stars;
        public int BestTurnCount;

        public RcStageProgress() { }

        public RcStageProgress(int stageId)
        {
            StageId = stageId;
            IsCleared = false;
            Stars = 0;
            BestTurnCount = int.MaxValue;
        }

        public void UpdateClear(int turnCount, int earnedStars)
        {
            IsCleared = true;

            if (turnCount < BestTurnCount)
                BestTurnCount = turnCount;

            if (earnedStars > Stars)
                Stars = earnedStars;
        }
    }
}
