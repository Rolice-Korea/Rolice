using System;
using System.Collections.Generic;
using Rolice.Data;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 채택 모델 구현 — 종류 해금 + 개수 보유 + 제거 시 반환.
    ///
    /// RcHomeData(DTO)를 직접 감싸고 조회 캐시만 따로 든다. 별도 런타임 모델을 두지 않는 이유는
    /// 개수 데이터가 작고, RcPlayerData.GetCurrency/SetCurrency와 같은 선례를 따르기 위함.
    /// (블록 배치 목록은 크고 이벤트가 붙어 RcHomeBuildManager가 Load/WriteTo를 유지 — 의도된 비대칭.)
    ///
    /// 배치는 소모가 아니라 재고 이동이다(제거 시 반환).
    /// 불변량: <b>섬에 배치된 모든 blockId는 해금 상태다</b> — 배치돼 있다는 건 이미 지급됐다는 뜻.
    /// 이게 깨지면 "제거는 되는데 다시 못 놓는" 블록 소실이 생기므로 생성 시점에 정합을 맞춘다.
    /// </summary>
    public class RcCountedBlockInventory : IRcBlockInventory
    {
        private readonly RcHomeData data;

        // DTO의 parallel list(JsonUtility가 Dictionary 직렬화 불가)를 O(1) 조회로 감싼 캐시.
        private readonly Dictionary<int, int> countCache = new();
        private readonly HashSet<int>         unlockedCache = new();

        public event Action OnChanged;

        public RcCountedBlockInventory(RcHomeData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            LoadAndNormalize();
        }

        public bool TracksCount => true;

        public bool IsUnlocked(int blockId) => unlockedCache.Contains(blockId);

        public int GetCount(int blockId) => countCache.TryGetValue(blockId, out int count) ? count : 0;

        public bool CanPlace(int blockId) => IsUnlocked(blockId) && GetCount(blockId) > 0;

        public void OnPlaced(int blockId)
        {
            int count = GetCount(blockId);
            if (count <= 0)
            {
                // 배치 커밋 전에 CanPlace로 걸러지는 게 정상 경로. 여기 도달 = 호출 순서 버그.
                Debug.LogWarning($"[BlockInventory] 재고 없는 블록 배치 커밋: {blockId}");
                return;
            }

            SetCount(blockId, count - 1);
            OnChanged?.Invoke();
        }

        public void OnRemoved(int blockId)
        {
            // 반환분이 해금 없는 상태로 쌓이면 CanPlace가 계속 false → 사실상 소실.
            // 배치돼 있었다는 것 자체가 지급의 증거이므로 해금도 함께 보장한다.
            UnlockInternal(blockId);
            SetCount(blockId, AddSaturating(GetCount(blockId), 1));
            OnChanged?.Invoke();
        }

        public void Unlock(int blockId)
        {
            if (UnlockInternal(blockId))
                OnChanged?.Invoke();
        }

        public void Grant(int blockId, int count)
        {
            if (count <= 0)
                return;

            UnlockInternal(blockId);
            SetCount(blockId, AddSaturating(GetCount(blockId), count));
            OnChanged?.Invoke();   // 해금·개수를 한 번에 반영(이벤트 중복 발행 방지)
        }

        // ─── DTO 동기화 ─────────────────────────────────────────────────────

        /// <summary>해금만 수행하고 실제 변화 여부를 돌려준다(이벤트는 호출자가 발행).</summary>
        private bool UnlockInternal(int blockId)
        {
            if (!unlockedCache.Add(blockId))
                return false;

            data.UnlockedBlockIds.Add(blockId);
            return true;
        }

        /// <summary>
        /// 개수 가산. 오버플로로 음수가 되면 그 블록은 CanPlace가 영영 false가 되어
        /// 사실상 소실되므로, 뒤집히는 대신 int.MaxValue에서 포화시킨다(손상 세이브 방어).
        /// </summary>
        private static int AddSaturating(int current, int delta)
        {
            long sum = (long)current + delta;
            if (sum > int.MaxValue) return int.MaxValue;
            if (sum < 0) return 0;
            return (int)sum;
        }

        private void SetCount(int blockId, int value)
        {
            countCache[blockId] = value;

            // parallel list 동기화 (RcPlayerData.SetCurrency와 동일 패턴)
            int idx = data.OwnedBlockIds.IndexOf(blockId);
            if (idx >= 0)
            {
                data.OwnedBlockCounts[idx] = value;
            }
            else
            {
                data.OwnedBlockIds.Add(blockId);
                data.OwnedBlockCounts.Add(value);
            }
        }

        // ─── 로드 / 정규화 ──────────────────────────────────────────────────

        /// <summary>
        /// DTO를 캐시로 읽어들이면서 손상된 세이브를 정규형으로 복구한다.
        /// 방어 대상: parallel list 길이 불일치(→ SetCount에서 인덱스 예외), 음수 개수,
        /// 해금 목록 중복, 그리고 "배치돼 있는데 해금 안 된" 구세이브.
        /// 정규화 결과를 DTO에 되써서 이후 어떤 경로로 저장되든 깨진 상태가 남지 않게 한다.
        /// </summary>
        private void LoadAndNormalize()
        {
            EnsureLists();

            countCache.Clear();

            // id는 전부 살리고, 짝이 없는 항목만 개수 0으로 본다(잘라내면 해금 이력까지 사라짐).
            // 중복 id는 마지막 값을 채택한다.
            for (int i = 0; i < data.OwnedBlockIds.Count; i++)
            {
                int count = i < data.OwnedBlockCounts.Count ? Math.Max(0, data.OwnedBlockCounts[i]) : 0;
                countCache[data.OwnedBlockIds[i]] = count;
            }

            unlockedCache.Clear();
            foreach (int blockId in data.UnlockedBlockIds)
                unlockedCache.Add(blockId);

            // 배치돼 있다는 건 이미 지급됐다는 뜻 → 해금 보장(위 불변량).
            // 개수는 올리지 않는다: 저장된 개수는 이미 배치분이 빠진 잔여값이므로.
            foreach (var block in data.Blocks)
                unlockedCache.Add(block.BlockId);

            WriteCachesToData();
        }

        /// <summary>캐시를 DTO에 그대로 반영(길이 정합·중복 제거·음수 제거된 정규형).</summary>
        private void WriteCachesToData()
        {
            data.OwnedBlockIds.Clear();
            data.OwnedBlockCounts.Clear();
            foreach (var pair in countCache)
            {
                data.OwnedBlockIds.Add(pair.Key);
                data.OwnedBlockCounts.Add(pair.Value);
            }

            data.UnlockedBlockIds.Clear();
            foreach (int blockId in unlockedCache)
                data.UnlockedBlockIds.Add(blockId);
        }

        /// <summary>구세이브/부분 초기화 방어 — DTO 리스트가 null이면 채운다.</summary>
        private void EnsureLists()
        {
            data.Blocks           ??= new List<RcPlacedBlock>();
            data.UnlockedBlockIds ??= new List<int>();
            data.OwnedBlockIds    ??= new List<int>();
            data.OwnedBlockCounts ??= new List<int>();
        }
    }
}
