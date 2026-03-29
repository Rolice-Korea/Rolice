using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rolice;

public static class RcLevelAutoGenerator
{
    public enum Preset
    {
        Full,       // 전체 격자 — 밀도로 랜덤 제거
        Path,       // 랜덤 워크 — 연결된 경로
        Cluster,    // 여러 클러스터 — 섬처럼 모여있음
        Hybrid,     // 역방향 핵심 구간 + 랜덤 혼합
    }

    public enum ShapePreset
    {
        Rectangle,  // 기본 (전체 격자)
        Diamond,    // 마름모
        Cross,      // 십자형
        Frame,      // 테두리
    }

    [Serializable]
    public class GenParams
    {
        public Preset      Preset          = Preset.Full;
        public ShapePreset Shape           = ShapePreset.Rectangle;
        public int         Width           = 6;
        public int         Height          = 6;
        public int         ColorCount      = 4;
        [Range(0.3f, 1f)]
        public float       FillRatio       = 0.7f;
        public float       TurnMultiplier  = 1.6f;
        public int         Seed            = -1;  // -1 = 매번 랜덤

        // Hybrid 전용
        public int    CriticalSegmentCount  = 1;
        [Range(3, 8)]
        public int    CriticalSegmentLength = 5;
    }

    // 실제 적용된 시드를 반환
    public static int Generate(RcLevelDataSO levelData, GenParams p, RcTileTypeSO colorTileType)
    {
        int seed = p.Seed >= 0 ? p.Seed : UnityEngine.Random.Range(0, 999999);
        var rng  = new System.Random(seed);

        levelData.Width  = Mathf.Max(3, p.Width);
        levelData.Height = Mathf.Max(3, p.Height);
        levelData.Tiles  = new RcTileData[levelData.Width * levelData.Height];

        bool[] active  = GenerateShape(p, rng, levelData.Width, levelData.Height);
        var    actives = new List<int>();
        for (int i = 0; i < active.Length; i++)
            if (active[i]) actives.Add(i);

        // ③ actives가 비어있으면 중앙 셀로 폴백
        if (actives.Count == 0)
        {
            int fallback = (levelData.Height / 2) * levelData.Width + (levelData.Width / 2);
            actives.Add(fallback);
        }

        // 스폰 위치: 활성 타일 중 랜덤 선택 (항상 갱신 보장)
        {
            int spawnIdx = actives[rng.Next(actives.Count)];
            levelData.SpawnGridPosition = new Vector2Int(spawnIdx % levelData.Width, spawnIdx / levelData.Width);
        }

        // ② Hybrid 역산 전에 다이스 초기 면을 먼저 임시 설정 (랜덤 색풀 기반)
        if (levelData.InitialDiceFaces == null || levelData.InitialDiceFaces.Length != 6)
            levelData.InitialDiceFaces = new RcColorType[6];
        {
            int clampedColorCount = Mathf.Clamp(p.ColorCount, 1, AllColors.Length);
            var tempPool = AllColors.ToList();
            Shuffle(tempPool, rng);
            for (int i = 0; i < 6; i++)
                levelData.InitialDiceFaces[i] = tempPool[i % clampedColorCount];
        }

        // Hybrid: 핵심 구간 먼저 역방향 설계
        var criticalSet = new HashSet<int>();
        if (p.Preset == Preset.Hybrid)
            ApplyCriticalSegments(levelData, actives, criticalSet, colorTileType, p, rng);

        // 나머지 타일 랜덤 배색
        var nonCritical = actives.Where(i => !criticalSet.Contains(i)).ToList();
        AssignColors(levelData, nonCritical, p.ColorCount, colorTileType, rng);

        int tileCount = actives.Count;
        int maxTurns  = Mathf.Max(tileCount + 2, Mathf.RoundToInt(tileCount * p.TurnMultiplier));
        levelData.Rules.HasTurnLimit = true;
        levelData.Rules.MaxTurns     = maxTurns;

        levelData.StageInfo ??= new Rolice.Data.RcStageInfo();
        levelData.StageInfo.MoveCountThreshold = tileCount * 3;          // ★2: 타일수 × 3회 이하
        levelData.StageInfo.TimeThreshold      = tileCount * 5f;         // ★3: 타일수 × 5초 이하

        // ① 실제 타일에 사용된 색 기반으로 다이스 면 최종 확정
        var tileColors = levelData.Tiles
            .Where(t => t != null && t.colorType != RcColorType.None)
            .Select(t => t.colorType)
            .Distinct()
            .ToList();

        if (tileColors.Count > 0)
        {
            // 6면에 tileColors를 순환 배치 — 모든 타일 색이 최소 1면에 존재
            for (int i = 0; i < 6; i++)
                levelData.InitialDiceFaces[i] = tileColors[i % tileColors.Count];
        }
        // tileColors가 비어있으면 임시 설정값 유지

        return seed;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shape Generation

    private static bool[] GenerateShape(GenParams p, System.Random rng, int w, int h)
    {
        bool[]    mask       = GetShapeMask(p.Shape, w, h);
        List<int> maskCells  = Enumerable.Range(0, w * h).Where(i => mask[i]).ToList();

        // ⑤ maskCells가 ColorCount보다 적을 수 있으므로 min을 maskCells.Count로 제한
        int safeColorCount = Mathf.Min(p.ColorCount, Mathf.Max(1, maskCells.Count));
        int targetCount    = Mathf.RoundToInt(maskCells.Count * Mathf.Clamp(p.FillRatio, 0.1f, 1f));
        targetCount        = Mathf.Clamp(targetCount, safeColorCount, maskCells.Count);

        return p.Preset switch
        {
            Preset.Full    => GenerateFull(rng, w, h, targetCount, maskCells),
            Preset.Path    => GeneratePath(rng, w, h, targetCount, mask, p.FillRatio >= 1f),
            Preset.Cluster => GenerateCluster(rng, w, h, targetCount, safeColorCount, mask),
            Preset.Hybrid  => GeneratePath(rng, w, h, targetCount, mask, p.FillRatio >= 1f),
            _              => GenerateFull(rng, w, h, targetCount, maskCells),
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shape Masks

    private static bool[] GetShapeMask(ShapePreset shape, int w, int h)
    {
        bool[] mask = new bool[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            mask[y * w + x] = shape switch
            {
                ShapePreset.Diamond => IsDiamond(x, y, w, h),
                ShapePreset.Cross   => IsCross(x, y, w, h),
                ShapePreset.Frame   => IsFrame(x, y, w, h),
                _                   => true,
            };
        return mask;
    }

    private static bool IsDiamond(int x, int y, int w, int h)
    {
        float cx = (w - 1) / 2f, cy = (h - 1) / 2f;
        return (Math.Abs(x - cx) / (w / 2f) + Math.Abs(y - cy) / (h / 2f)) <= 1f;
    }

    private static bool IsCross(int x, int y, int w, int h)
    {
        int midX1 = (w - 1) / 2, midX2 = w / 2;
        int midY1 = (h - 1) / 2, midY2 = h / 2;
        return (x >= midX1 && x <= midX2) || (y >= midY1 && y <= midY2);
    }

    private static bool IsFrame(int x, int y, int w, int h) =>
        x == 0 || x == w - 1 || y == 0 || y == h - 1;

    // ─────────────────────────────────────────────────────────────────────────

    private static bool[] GenerateFull(System.Random rng, int w, int h, int target, List<int> candidates)
    {
        bool[] active   = new bool[w * h];
        var    shuffled = candidates.ToList();
        Shuffle(shuffled, rng);
        int keep = Math.Min(target, shuffled.Count);
        for (int i = 0; i < keep; i++) active[shuffled[i]] = true;
        return active;
    }

    private static bool[] GeneratePath(System.Random rng, int w, int h, int target, bool[] mask, bool fillAll = false)
    {
        bool[] active   = new bool[w * h];
        var    maskList = Enumerable.Range(0, w * h).Where(i => mask[i]).ToList();
        if (maskList.Count == 0) return active;

        // ④ fillAll=true이면 모든 마스크 셀을 활성화 (FillRatio=1 보장)
        if (fillAll)
        {
            foreach (var idx in maskList) active[idx] = true;
            return active;
        }

        int    cur     = maskList[rng.Next(maskList.Count)];
        active[cur]    = true;
        int placed     = 1;
        int maxIter    = w * h * 20;
        int[] dirs     = { -1, 1, -w, w };

        for (int iter = 0; iter < maxIter && placed < target; iter++)
        {
            int dir  = dirs[rng.Next(dirs.Length)];
            int next = cur + dir;
            if (next < 0 || next >= w * h) continue;
            if (Math.Abs((next % w) - (cur % w)) > 1) continue;
            if (!mask[next]) continue;

            cur = next;
            if (!active[cur]) { active[cur] = true; placed++; }
        }

        return active;
    }

    private static bool[] GenerateCluster(System.Random rng, int w, int h, int target, int colorCount, bool[] mask)
    {
        bool[] active   = new bool[w * h];
        var    maskList = Enumerable.Range(0, w * h).Where(i => mask[i]).ToList();
        if (maskList.Count == 0) return active;

        var frontier = new List<int>();
        int clusters = Mathf.Max(2, Mathf.Min(colorCount, target / 3 + 1));

        var usedSeeds = new HashSet<int>();
        for (int i = 0; i < clusters; i++)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                int idx = maskList[rng.Next(maskList.Count)];
                if (!usedSeeds.Contains(idx))
                {
                    usedSeeds.Add(idx);
                    active[idx] = true;
                    frontier.Add(idx);
                    break;
                }
            }
        }

        int placed = frontier.Count;

        while (placed < target && frontier.Count > 0)
        {
            int pick = rng.Next(frontier.Count);
            int cell = frontier[pick];
            int cx   = cell % w;
            int cy   = cell / w;

            var neighbors = new List<int>();
            if (cx > 0     && !active[cell - 1] && mask[cell - 1]) neighbors.Add(cell - 1);
            if (cx < w - 1 && !active[cell + 1] && mask[cell + 1]) neighbors.Add(cell + 1);
            if (cy > 0     && !active[cell - w] && mask[cell - w]) neighbors.Add(cell - w);
            if (cy < h - 1 && !active[cell + w] && mask[cell + w]) neighbors.Add(cell + w);

            if (neighbors.Count == 0) { frontier.RemoveAt(pick); continue; }

            int chosen = neighbors[rng.Next(neighbors.Count)];
            active[chosen] = true;
            frontier.Add(chosen);
            placed++;
        }

        return active;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Hybrid: 역방향 핵심 구간

    private static void ApplyCriticalSegments(
        RcLevelDataSO  levelData,
        List<int>      actives,
        HashSet<int>   criticalSet,
        RcTileTypeSO   tileType,
        GenParams      p,
        System.Random  rng)
    {
        int tileCount = Mathf.RoundToInt(levelData.Width * levelData.Height * Mathf.Clamp(p.FillRatio, 0.1f, 1f));
        int maxSegCount = Mathf.Max(1, tileCount / (Mathf.Clamp(p.CriticalSegmentLength, 3, 8) * 2));
        int segCount = Mathf.Clamp(p.CriticalSegmentCount, 1, maxSegCount);
        int segLen   = Mathf.Clamp(p.CriticalSegmentLength, 3, 8);

        for (int s = 0; s < segCount; s++)
        {
            // 이미 지정된 타일 제외하고 탐색
            var available = actives.Where(i => !criticalSet.Contains(i)).ToList();
            var segment   = FindSimplePath(levelData, available, segLen, rng);
            if (segment == null) continue;

            ApplyCriticalSegment(levelData, segment, tileType, rng);
            foreach (var idx in segment) criticalSet.Add(idx);
        }
    }

    // DFS로 length 길이의 단순 경로 탐색
    private static List<int> FindSimplePath(
        RcLevelDataSO  ld,
        List<int>      available,
        int            length,
        System.Random  rng)
    {
        if (available.Count < length) return null;

        var activeSet = new HashSet<int>(available);

        for (int attempt = 0; attempt < 60; attempt++)
        {
            int start   = available[rng.Next(available.Count)];
            var path    = new List<int> { start };
            var visited = new HashSet<int> { start };

            if (DFSSimplePath(ld, start, length - 1, visited, path, activeSet, rng))
                return path;
        }

        return null;
    }

    private static bool DFSSimplePath(
        RcLevelDataSO  ld,
        int            cur,
        int            remaining,
        HashSet<int>   visited,
        List<int>      path,
        HashSet<int>   activeSet,
        System.Random  rng)
    {
        if (remaining == 0) return true;

        var neighbors = ShuffledNeighbors(ld, cur, activeSet, rng);
        foreach (var n in neighbors)
        {
            if (visited.Contains(n)) continue;
            visited.Add(n);
            path.Add(n);
            if (DFSSimplePath(ld, n, remaining - 1, visited, path, activeSet, rng))
                return true;
            visited.Remove(n);
            path.RemoveAt(path.Count - 1);
        }
        return false;
    }

    // 핵심 구간에 다이스 역산으로 타일 색 배정
    private static void ApplyCriticalSegment(
        RcLevelDataSO  ld,
        List<int>      segment,
        RcTileTypeSO   tileType,
        System.Random  rng)
    {
        // 랜덤 회전으로 진입 다이스 상태 생성 (중간 경유 시뮬레이션)
        var dice    = new RcDiceFaceData(ld.InitialDiceFaces);
        var allDirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int rotations = rng.Next(3, 10);
        for (int i = 0; i < rotations; i++)
            dice = dice.Rotate(allDirs[rng.Next(4)]);

        // 첫 타일: 진입 시 바닥면 색
        ld.Tiles[segment[0]] = new RcTileData
        {
            TileType  = tileType,
            colorType = dice.GetBottomColor(),
        };

        // 이후 타일: 경로 방향으로 굴리며 바닥면 역산
        for (int i = 1; i < segment.Count; i++)
        {
            var dir = new Vector2Int(
                segment[i] % ld.Width  - segment[i - 1] % ld.Width,
                segment[i] / ld.Width  - segment[i - 1] / ld.Width
            );
            dice = dice.Rotate(dir);
            ld.Tiles[segment[i]] = new RcTileData
            {
                TileType  = tileType,
                colorType = dice.GetBottomColor(),
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Color Assignment

    private static readonly RcColorType[] AllColors =
    {
        RcColorType.White, RcColorType.Magenta, RcColorType.Yellow,
        RcColorType.Green, RcColorType.Cyan,    RcColorType.Grey
    };

    private static void AssignColors(
        RcLevelDataSO ld, List<int> cells, int colorCount,
        RcTileTypeSO tileType, System.Random rng)
    {
        colorCount = Mathf.Clamp(colorCount, 1, AllColors.Length);
        var colorPool = AllColors.ToList();
        Shuffle(colorPool, rng);
        var usedColors = colorPool.Take(colorCount).ToArray();

        var shuffled = cells.ToList();
        Shuffle(shuffled, rng);

        for (int i = 0; i < shuffled.Count; i++)
            ld.Tiles[shuffled[i]] = new RcTileData { TileType = tileType, colorType = usedColors[i % colorCount] };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers

    private static List<int> ShuffledNeighbors(
        RcLevelDataSO ld, int idx, HashSet<int> activeSet, System.Random rng)
    {
        int cx = idx % ld.Width, cy = idx / ld.Width;
        var result = new List<int>(4);
        int[] dx = {  0, 0, 1, -1 };
        int[] dy = {  1, -1, 0, 0 };

        for (int d = 0; d < 4; d++)
        {
            int nx = cx + dx[d], ny = cy + dy[d];
            if (nx < 0 || nx >= ld.Width || ny < 0 || ny >= ld.Height) continue;
            int ni = ny * ld.Width + nx;
            if (activeSet.Contains(ni)) result.Add(ni);
        }
        Shuffle(result, rng);
        return result;
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Scaled Params (stage 1 → 100 난이도 자동 스케일)

    public static GenParams BuildScaledParams(int stageNumber, System.Random rng, int minGrid = 3, int maxGrid = 8)
    {
        float t = Mathf.Clamp01((stageNumber - 1) / 99f);

        var p = new GenParams();

        // 그리드: minGrid → maxGrid
        int size  = Mathf.RoundToInt(Mathf.Lerp(minGrid, maxGrid, t));
        p.Width   = size;
        p.Height  = size;

        // 색 수: 2 → 6
        p.ColorCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(2f, 6f, t)), 2, 6);

        // 밀도: 100% 고정
        p.FillRatio = 1f;

        // 턴 여유: 2.0 → 1.4 (후반일수록 타이트)
        p.TurnMultiplier = Mathf.Lerp(2.0f, 1.4f, t);

        // Preset 랜덤 (초반은 Full 비중 높게)
        float roll = (float)rng.NextDouble();
        if (t < 0.2f)
            p.Preset = roll < 0.7f ? Preset.Full : Preset.Path;
        else if (t < 0.5f)
            p.Preset = roll < 0.4f ? Preset.Full : (roll < 0.7f ? Preset.Path : Preset.Cluster);
        else
        {
            var presets = (Preset[])System.Enum.GetValues(typeof(Preset));
            p.Preset = presets[rng.Next(presets.Length)];
        }

        // Shape 랜덤 (초반은 Rectangle 고정)
        if (t < 0.15f)
            p.Shape = ShapePreset.Rectangle;
        else
        {
            var shapes = (ShapePreset[])System.Enum.GetValues(typeof(ShapePreset));
            p.Shape = shapes[rng.Next(shapes.Length)];
        }

        // Hybrid 전용
        p.CriticalSegmentLength = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(3f, 7f, t)), 3, 8);
        p.CriticalSegmentCount  = 1;

        p.Seed = -1;
        return p;
    }
}
