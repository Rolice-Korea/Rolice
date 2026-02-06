using UnityEngine;

namespace Rolice.Data
{
    /// <summary>
    /// 모든 스테이지(레벨) 목록을 관리하는 마스터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StageDatabase", menuName = "Rolice/Stage Database")]
    public class RcStageDatabaseSO : ScriptableObject
    {
        [Tooltip("스테이지 순서대로 배열. 인덱스 = 스테이지 번호 - 1")]
        public RcLevelDataSO[] Stages;

        /// <summary>
        /// 전체 스테이지 수
        /// </summary>
        public int StageCount => Stages?.Length ?? 0;

        /// <summary>
        /// 스테이지 번호로 레벨 데이터 조회 (1-based)
        /// </summary>
        public RcLevelDataSO GetStage(int stageNumber)
        {
            int index = stageNumber - 1;
            if (Stages == null || index < 0 || index >= Stages.Length)
                return null;

            return Stages[index];
        }

        /// <summary>
        /// 인덱스로 레벨 데이터 조회 (0-based)
        /// </summary>
        public RcLevelDataSO GetStageByIndex(int index)
        {
            if (Stages == null || index < 0 || index >= Stages.Length)
                return null;

            return Stages[index];
        }

        private void OnValidate()
        {
            // 에디터에서 스테이지 번호 자동 할당
            if (Stages == null) return;

            for (int i = 0; i < Stages.Length; i++)
            {
                if (Stages[i] != null && Stages[i].StageInfo != null)
                {
                    Stages[i].StageInfo.StageNumber = i + 1;
                }
            }
        }
    }
}
