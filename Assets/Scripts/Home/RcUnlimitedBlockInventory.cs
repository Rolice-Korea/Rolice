using System;
using System.Collections.Generic;
using Rolice.Data;

namespace Rolice.Home
{
    /// <summary>
    /// 뒤집힘 대비 구현 — 종류만 해금하면 무제한 사용. 개수/반환 개념 없음.
    ///
    /// 현재 채택 모델은 개수형(RcCountedBlockInventory)이며, 이 구현은 두 용도로 존재한다.
    /// 1) 획득 모델이 무한형으로 뒤집힐 때 주입 대상만 바꾸면 되도록 하는 보험
    /// 2) 개발용 하네스(HomeScene 등)에서 재고에 막히지 않고 배치를 시험할 때
    /// </summary>
    public class RcUnlimitedBlockInventory : IRcBlockInventory
    {
        private readonly RcHomeData data;

        // IsUnlocked가 배치 커서 갱신 경로(매 프레임)에서 불리므로 List 선형 탐색을 피한다.
        private readonly HashSet<int> unlockedCache = new();

        public event Action OnChanged;

        public RcUnlimitedBlockInventory(RcHomeData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            LoadAndNormalize();
        }

        public bool TracksCount => false;

        public bool IsUnlocked(int blockId) => unlockedCache.Contains(blockId);

        /// <summary>무제한이므로 상한값을 돌려준다 — UI는 TracksCount로 분기할 것(그대로 그리면 안 됨).</summary>
        public int GetCount(int blockId) => IsUnlocked(blockId) ? int.MaxValue : 0;

        public bool CanPlace(int blockId) => IsUnlocked(blockId);

        public void OnPlaced(int blockId) { }

        public void OnRemoved(int blockId) { }

        public void Unlock(int blockId)
        {
            if (!unlockedCache.Add(blockId))
                return;

            data.UnlockedBlockIds.Add(blockId);
            OnChanged?.Invoke();
        }

        /// <summary>무한형에서 개수는 의미가 없으므로 해금만 수행한다.</summary>
        public void Grant(int blockId, int count) => Unlock(blockId);

        /// <summary>
        /// 해금 목록을 캐시로 읽으며 중복/null을 정리하고, 배치된 블록 종류의 해금을 보장한다
        /// (구세이브 정합 — RcCountedBlockInventory와 동일 규약).
        /// </summary>
        private void LoadAndNormalize()
        {
            data.Blocks           ??= new List<RcPlacedBlock>();
            data.UnlockedBlockIds ??= new List<int>();

            unlockedCache.Clear();
            foreach (int blockId in data.UnlockedBlockIds)
                unlockedCache.Add(blockId);

            foreach (var block in data.Blocks)
                unlockedCache.Add(block.BlockId);

            data.UnlockedBlockIds.Clear();
            foreach (int blockId in unlockedCache)
                data.UnlockedBlockIds.Add(blockId);
        }
    }
}
