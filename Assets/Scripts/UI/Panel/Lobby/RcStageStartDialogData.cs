namespace Rolice.UI
{
    public struct RcStageStartDialogData
    {
        public int   StageNumber;
        public string StageName;
        public int   MoveCountThreshold;  // 0 = 비활성
        public float TimeThreshold;       // 0 = 비활성
        public int   CurrentStars;
    }
}
