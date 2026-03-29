using System.Collections.Generic;
using Rolice;
using UnityEngine;

public static class RcLevelValidator
{
    public struct Result
    {
        public bool   IsValid;
        public int    TotalTiles;
        public int    TotalColorTiles;
        public Dictionary<RcColorType, int> TileColorCounts;
        public Dictionary<RcColorType, int> DiceColorCounts;
        public bool   ConnectivityOk;
        public bool   ColorCoverageOk;
        public int    SolveRate;      // 0~100 (%), -1 = 미측정
        public int    BestClearRate;  // 0~100 (%)
        public List<string> Errors;
        public List<string> Infos;
    }

    const int SIM_COUNT = 200;

    static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

    static readonly System.Random Rng = new System.Random();

    // ─────────────────────────────────────────────────────────────
    public static Result Validate(RcLevelDataSO ld)
    {
        var r = new Result
        {
            TileColorCounts = new Dictionary<RcColorType, int>(),
            DiceColorCounts = new Dictionary<RcColorType, int>(),
            Errors          = new List<string>(),
            Infos           = new List<string>(),
            SolveRate       = -1,
            BestClearRate   = -1,
        };

        if (ld == null || ld.Tiles == null) return r;

        // ── 1. 타일 집계 ─────────────────────────────────────────
        var allTiles   = new List<int>();
        var colorTiles = new List<int>();

        for (int i = 0; i < ld.Tiles.Length; i++)
        {
            var t = ld.Tiles[i];
            if (t == null || t.IsEmpty) continue;
            allTiles.Add(i);
            if (t.TileType != null && t.TileType.bHasColor && t.colorType != RcColorType.None)
            {
                colorTiles.Add(i);
                if (!r.TileColorCounts.ContainsKey(t.colorType)) r.TileColorCounts[t.colorType] = 0;
                r.TileColorCounts[t.colorType]++;
            }
        }

        r.TotalTiles      = allTiles.Count;
        r.TotalColorTiles = colorTiles.Count;

        // ── 2. 다이스 면 집계 ────────────────────────────────────
        if (ld.InitialDiceFaces != null)
        {
            foreach (var c in ld.InitialDiceFaces)
            {
                if (c == RcColorType.None) continue;
                if (!r.DiceColorCounts.ContainsKey(c)) r.DiceColorCounts[c] = 0;
                r.DiceColorCounts[c]++;
            }
        }

        if (allTiles.Count == 0) return r;

        // ── 3. 색상 커버리지 ─────────────────────────────────────
        r.ColorCoverageOk = true;
        foreach (var c in r.TileColorCounts.Keys)
        {
            if (!r.DiceColorCounts.ContainsKey(c))
            {
                r.Errors.Add($"타일 색 [{c}] 이 다이스 면에 없습니다.");
                r.ColorCoverageOk = false;
            }
        }

        // ── 4. 연결성 ─────────────────────────────────────────────
        r.ConnectivityOk = CheckConnectivity(ld, allTiles);
        if (!r.ConnectivityOk)
            r.Errors.Add("고립된 타일 존재 — 다이스가 도달할 수 없는 영역이 있습니다.");

        // ── 5. 시뮬레이션 ────────────────────────────────────────
        if (r.Errors.Count == 0 && colorTiles.Count > 0 && ld.InitialDiceFaces != null)
        {
            int solved    = 0;
            int bestClear = 0;

            for (int s = 0; s < SIM_COUNT; s++)
            {
                int start   = allTiles[Rng.Next(allTiles.Count)];
                int cleared = RunGreedySim(ld, allTiles, colorTiles, start);
                if (cleared == colorTiles.Count) solved++;
                if (cleared > bestClear) bestClear = cleared;
            }

            r.SolveRate     = solved * 100 / SIM_COUNT;
            r.BestClearRate = colorTiles.Count > 0 ? bestClear * 100 / colorTiles.Count : 0;

            if (r.SolveRate == 0 && r.BestClearRate < 80)
                r.Errors.Add($"시뮬레이션 {SIM_COUNT}회 중 클리어 0회 — 클리어 불가능 가능성.");
            else if (r.SolveRate < 20)
                r.Infos.Add($"클리어율 낮음 ({r.SolveRate}%). 턴 수나 맵 구조 재검토 권장.");
        }

        r.IsValid = r.Errors.Count == 0;
        return r;
    }

    // ─────────────────────────────────────────────────────────────
    static bool CheckConnectivity(RcLevelDataSO ld, List<int> active)
    {
        if (active.Count <= 1) return true;

        var set     = new HashSet<int>(active);
        var visited = new HashSet<int>();
        var queue   = new Queue<int>();

        queue.Enqueue(active[0]);
        visited.Add(active[0]);

        while (queue.Count > 0)
        {
            int cur = queue.Dequeue();
            int cx  = cur % ld.Width;
            int cy  = cur / ld.Width;

            foreach (var d in Dirs)
            {
                int nx = cx + d.x, ny = cy + d.y;
                if (nx < 0 || nx >= ld.Width || ny < 0 || ny >= ld.Height) continue;
                int ni = ny * ld.Width + nx;
                if (!set.Contains(ni) || visited.Contains(ni)) continue;
                visited.Add(ni);
                queue.Enqueue(ni);
            }
        }

        return visited.Count == active.Count;
    }

    // ─────────────────────────────────────────────────────────────
    // 그리디 시뮬레이션:
    //   1순위 — 굴리면 바닥면이 해당 타일 색과 일치하는 인접 타일로 이동
    //   2순위 — 무작위 이동
    static int RunGreedySim(
        RcLevelDataSO ld,
        List<int>     allTiles,
        List<int>     colorTiles,
        int           startIdx)
    {
        var allSet   = new HashSet<int>(allTiles);
        var colorSet = new HashSet<int>(colorTiles);
        var cleared  = new HashSet<int>();

        var dice = new RcDiceFaceData(ld.InitialDiceFaces);
        int pos  = startIdx;

        // 시작 타일 클리어 시도
        TryClear(ld, pos, dice, colorSet, cleared);

        int maxTurns = ld.Rules.HasTurnLimit ? ld.Rules.MaxTurns : 40;

        for (int t = 0; t < maxTurns && cleared.Count < colorTiles.Count; t++)
        {
            var neighbors = GetNeighbors(ld, pos, allSet);
            if (neighbors.Count == 0) break;

            // 1순위: 굴렸을 때 미클리어 타일을 지울 수 있는 이웃
            var greenOptions = new List<int>();
            foreach (var n in neighbors)
            {
                var newDice = dice.Rotate(IndexToDir(ld, pos, n));
                var tile    = GetTileData(ld, n);
                if (tile != null && colorSet.Contains(n) && !cleared.Contains(n)
                    && newDice.GetBottomColor() == tile.colorType)
                    greenOptions.Add(n);
            }

            int chosen = greenOptions.Count > 0
                ? greenOptions[Rng.Next(greenOptions.Count)]
                : neighbors[Rng.Next(neighbors.Count)];

            dice = dice.Rotate(IndexToDir(ld, pos, chosen));
            pos  = chosen;
            TryClear(ld, pos, dice, colorSet, cleared);
        }

        return cleared.Count;
    }

    // ─────────────────────────────────────────────────────────────
    static void TryClear(
        RcLevelDataSO    ld,
        int              idx,
        RcDiceFaceData   dice,
        HashSet<int>     colorSet,
        HashSet<int>     cleared)
    {
        if (!colorSet.Contains(idx) || cleared.Contains(idx)) return;
        var tile = GetTileData(ld, idx);
        if (tile != null && tile.TileType != null && tile.TileType.bHasColor
            && dice.GetBottomColor() == tile.colorType)
            cleared.Add(idx);
    }

    static List<int> GetNeighbors(RcLevelDataSO ld, int idx, HashSet<int> allSet)
    {
        var result = new List<int>(4);
        int cx = idx % ld.Width, cy = idx / ld.Width;
        foreach (var d in Dirs)
        {
            int nx = cx + d.x, ny = cy + d.y;
            if (nx < 0 || nx >= ld.Width || ny < 0 || ny >= ld.Height) continue;
            int ni = ny * ld.Width + nx;
            if (allSet.Contains(ni)) result.Add(ni);
        }
        return result;
    }

    static Vector2Int IndexToDir(RcLevelDataSO ld, int from, int to) =>
        new Vector2Int(to % ld.Width - from % ld.Width, to / ld.Width - from / ld.Width);

    static RcTileData GetTileData(RcLevelDataSO ld, int idx) =>
        (idx >= 0 && idx < ld.Tiles.Length) ? ld.Tiles[idx] : null;
}
