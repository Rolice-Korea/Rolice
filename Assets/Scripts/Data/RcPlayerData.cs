using System;
using System.Collections.Generic;

namespace Rolice.Data
{
    [Serializable]
    public class RcPlayerData
    {
        public int Version = 1;
        
        public RcFaceSkinType SelectedFaceSkin = RcFaceSkinType.Default;
        public RcEdgeSkinType SelectedEdgeSkin = RcEdgeSkinType.Default;

        public List<RcStageProgress> StageProgressList = new();

        // --- 재화 ---
        // JsonUtility는 Dictionary 직렬화 불가 → parallel list 패턴 사용
        public List<string> CurrencyKeys   = new();
        public List<int>    CurrencyValues = new();

        // --- 보유 아이템 ---
        // JsonUtility는 HashSet 직렬화 불가 → List로 저장 후 캐시에서 HashSet 복원
        public List<string> OwnedItemsList = new();

        // --- 홈(마을 꾸미기) 레이아웃 ---
        // 런타임 그리드 모델은 RcHomeBuildManager가 소유. 여기는 직렬화 DTO만 보관.
        public RcHomeData Home = new();

        [NonSerialized]
        private Dictionary<int, RcStageProgress> progressCache;

        [NonSerialized]
        private Dictionary<string, int> currencyCache;

        [NonSerialized]
        private HashSet<string> ownedItemsCache;

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

        /// <summary>전 스테이지 누적 별 합. 상점 스킨 해금 게이트(RequiredStars) 판정용.</summary>
        public int GetTotalStars()
        {
            int total = 0;
            foreach (var progress in StageProgressList)
                total += progress.Stars;
            return total;
        }

        // ─── 재화 접근 ─────────────────────────────────────────────────────

        public int GetCurrency(string key)
        {
            EnsureCurrencyCache();
            return currencyCache.TryGetValue(key, out int value) ? value : 0;
        }

        public void SetCurrency(string key, int value)
        {
            EnsureCurrencyCache();

            currencyCache[key] = value;

            // parallel list 동기화
            int idx = CurrencyKeys.IndexOf(key);
            if (idx >= 0)
            {
                CurrencyValues[idx] = value;
            }
            else
            {
                CurrencyKeys.Add(key);
                CurrencyValues.Add(value);
            }
        }

        // ─── 아이템 접근 ────────────────────────────────────────────────────

        public bool HasOwnedItem(string itemId)
        {
            EnsureOwnedItemsCache();
            return ownedItemsCache.Contains(itemId);
        }

        public void AddOwnedItem(string itemId)
        {
            EnsureOwnedItemsCache();

            if (ownedItemsCache.Add(itemId))
                OwnedItemsList.Add(itemId);
        }

        // ─── 캐시 관리 ──────────────────────────────────────────────────────

        public void RebuildCache()
        {
            progressCache = new Dictionary<int, RcStageProgress>();
            foreach (var progress in StageProgressList)
                progressCache[progress.StageId] = progress;

            currencyCache = new Dictionary<string, int>(CurrencyKeys.Count);
            for (int i = 0; i < CurrencyKeys.Count && i < CurrencyValues.Count; i++)
                currencyCache[CurrencyKeys[i]] = CurrencyValues[i];

            ownedItemsCache = new HashSet<string>(OwnedItemsList);
        }

        private void EnsureCache()
        {
            if (progressCache == null)
                RebuildCache();
        }

        private void EnsureCurrencyCache()
        {
            if (currencyCache == null)
                RebuildCache();
        }

        private void EnsureOwnedItemsCache()
        {
            if (ownedItemsCache == null)
                RebuildCache();
        }

        public static RcPlayerData CreateNew()
        {
            return new RcPlayerData
            {
                Version           = 1,
                StageProgressList = new List<RcStageProgress>(),
                CurrencyKeys      = new List<string>(),
                CurrencyValues    = new List<int>(),
                OwnedItemsList    = new List<string>(),
                Home              = new RcHomeData(),
            };
        }
    }
}
