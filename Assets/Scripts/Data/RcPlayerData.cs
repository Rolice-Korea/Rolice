using System;
using System.Collections.Generic;

namespace Rolice.Data
{
    /// <summary>
    /// 전체 플레이어 진행 데이터
    /// </summary>
    [Serializable]
    public class RcPlayerData
    {
        public int Version = 1;
        public List<RcStageProgress> StageProgressList = new();

        // 런타임용 (직렬화 제외)
        [NonSerialized]
        private Dictionary<int, RcStageProgress> _progressCache;

        /// <summary>
        /// 스테이지 진행 상황 조회. 없으면 새로 생성
        /// </summary>
        public RcStageProgress GetProgress(int stageId)
        {
            EnsureCache();

            if (_progressCache.TryGetValue(stageId, out var progress))
                return progress;

            // 새 진행 상황 생성
            progress = new RcStageProgress(stageId);
            StageProgressList.Add(progress);
            _progressCache[stageId] = progress;

            return progress;
        }

        /// <summary>
        /// 스테이지 클리어 여부
        /// </summary>
        public bool IsStageCleared(int stageId)
        {
            EnsureCache();
            return _progressCache.TryGetValue(stageId, out var progress) && progress.IsCleared;
        }

        /// <summary>
        /// 스테이지 별 개수
        /// </summary>
        public int GetStageStars(int stageId)
        {
            EnsureCache();
            return _progressCache.TryGetValue(stageId, out var progress) ? progress.Stars : 0;
        }

        /// <summary>
        /// 로드 후 캐시 재구성
        /// </summary>
        public void RebuildCache()
        {
            _progressCache = new Dictionary<int, RcStageProgress>();
            foreach (var progress in StageProgressList)
            {
                _progressCache[progress.StageId] = progress;
            }
        }

        private void EnsureCache()
        {
            if (_progressCache == null)
                RebuildCache();
        }

        /// <summary>
        /// 새 플레이어 데이터 생성
        /// </summary>
        public static RcPlayerData CreateNew()
        {
            return new RcPlayerData
            {
                Version = 1,
                StageProgressList = new List<RcStageProgress>()
            };
        }
    }
}
