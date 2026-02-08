using System;
using UnityEngine;

namespace Rolice.Data
{
    /// <summary>
    /// 스테이지 메타 정보 (레벨 선택 화면용)
    /// </summary>
    [Serializable]
    public class RcStageInfo
    {
        [Tooltip("스테이지 번호 (Database에서 자동 할당)")]
        public int StageNumber;

        [Tooltip("스테이지 표시 이름 (비어있으면 번호 사용)")]
        public string DisplayName;

        [Tooltip("최대 별 개수")]
        public int MaxStars = 3;

        [Tooltip("별 획득 기준 턴 수 (3성, 2성, 1성 순서)")]
        public int[] StarThresholds = { 10, 15, 20 };

        /// <summary>
        /// 표시용 이름 반환
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(DisplayName) ? $"Stage {StageNumber}" : DisplayName;
        }

        /// <summary>
        /// 턴 수 기반 별 계산
        /// </summary>
        public int CalculateStars(int turnCount)
        {
            if (StarThresholds == null || StarThresholds.Length == 0)
                return 1;

            for (int i = 0; i < StarThresholds.Length; i++)
            {
                if (turnCount <= StarThresholds[i])
                    return MaxStars - i;
            }

            return 1; // 최소 1성
        }

        public void Validate()
        {
            MaxStars = Mathf.Clamp(MaxStars, 1, 3);

            if (StarThresholds == null || StarThresholds.Length != MaxStars)
            {
                StarThresholds = new int[MaxStars];
                for (int i = 0; i < MaxStars; i++)
                {
                    StarThresholds[i] = 10 + (i * 5);
                }
            }
        }
    }
}
