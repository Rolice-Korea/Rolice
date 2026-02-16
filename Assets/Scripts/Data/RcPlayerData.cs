using System;
using System.Collections.Generic;

namespace Rolice.Data
{
    [Serializable]
    public class RcPlayerData
    {
        public int Version = 1;
        public List<RcStageProgress> StageProgressList = new();

        [NonSerialized]
        private Dictionary<int, RcStageProgress> progressCache;

        public RcStageProgress GetProgress(int stageId)
        {
            EnsureCache();

            if (progressCache.TryGetValue(stageId, out var progress))
                return progress;

            progress = new RcStageProgress(stageId);
            StageProgressList.Add(progress);
            progressCache[stageId] = progress;

            return progress;
        }

        public bool IsStageCleared(int stageId)
        {
            EnsureCache();
            return progressCache.TryGetValue(stageId, out var progress) && progress.IsCleared;
        }

        public int GetStageStars(int stageId)
        {
            EnsureCache();
            return progressCache.TryGetValue(stageId, out var progress) ? progress.Stars : 0;
        }

        public void RebuildCache()
        {
            progressCache = new Dictionary<int, RcStageProgress>();
            foreach (var progress in StageProgressList)
            {
                progressCache[progress.StageId] = progress;
            }
        }

        private void EnsureCache()
        {
            if (progressCache == null)
                RebuildCache();
        }

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
