using System;

namespace Rolice.Data
{
    [Serializable]
    public class RcStageProgress
    {
        public int   StageId;
        public bool  IsCleared;
        public int   Stars;
        public int   BestMoveCount;
        public float BestTime;

        public RcStageProgress() { }

        public RcStageProgress(int stageId)
        {
            StageId       = stageId;
            IsCleared     = false;
            Stars         = 0;
            BestMoveCount = int.MaxValue;
            BestTime      = float.MaxValue;
        }

        public void UpdateClear(int moveCount, float elapsedTime, int earnedStars)
        {
            IsCleared = true;

            if (moveCount < BestMoveCount)
                BestMoveCount = moveCount;

            if (elapsedTime < BestTime)
                BestTime = elapsedTime;

            if (earnedStars > Stars)
                Stars = earnedStars;
        }
    }
}
