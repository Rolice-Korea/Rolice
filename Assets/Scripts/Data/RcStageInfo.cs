using System;
using UnityEngine;

namespace Rolice.Data
{
    [Serializable]
    public class RcStageInfo
    {
        [Tooltip("스테이지 번호 (Database에서 자동 할당)")]
        public int StageNumber;

        [Tooltip("스테이지 표시 이름 (비어있으면 번호 사용)")]
        public string DisplayName;

        [Header("Star Conditions")]
        [Tooltip("별 2: 이 횟수 이하로 클리어 시 획득 (0 = 비활성)")]
        public int MoveCountThreshold = 0;

        [Tooltip("별 3: 이 시간(초) 이하로 클리어 시 획득 (0 = 비활성)")]
        public float TimeThreshold = 0f;

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) ? $"Stage {StageNumber}" : DisplayName;
        }

        // 별 1: 클리어 자체 / 별 2: 횟수 조건 / 별 3: 시간 조건
        public int CalculateStars(int moveCount, float elapsedTime)
        {
            int stars = 1; // 클리어하면 무조건 1성

            if (MoveCountThreshold > 0 && moveCount <= MoveCountThreshold)
                stars++;

            if (TimeThreshold > 0f && elapsedTime <= TimeThreshold)
                stars++;

            return stars;
        }

        public void Validate() { }
    }
}
