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
    }

    [Serializable]
    public class GenParams
    {
        public Preset Preset         = Preset.Full;
        public int    Width          = 6;
        public int    Height         = 6;
        public int    ColorCount     = 4;
        [Range(0.3f, 1f)]
        public float  FillRatio      = 0.7f;
        public float  TurnMultiplier = 1.6f;
        public int    Seed           = -1;  // -1 = 매번 랜덤
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

        AssignColors(levelData, actives, p.ColorCount, colorTileType, rng);

        int tileCount = actives.Count;
        int maxTurns  = Mathf.Max(tileCount + 2, Mathf.RoundToInt(tileCount * p.TurnMultiplier));
        levelData.Rules.HasTurnLimit = true;
        levelData.Rules.MaxTurns     = maxTurns;

        // 반드시 오름차순 — CalculateStars가 순서 의존
        levelData.StageInfo ??= new Rolice.Data.RcStageInfo();
        levelData.StageInfo.MaxStars = 3;
        int star3 = tileCount + 1;
        int star2 = Mathf.Max(star3 + 1, Mathf.RoundToInt(tileCount * 1.25f) + 1);
        int star1 = Mathf.Max(star2 + 1, maxTurns - 1);
        levelData.StageInfo.StarThresholds = new[] { star3, star2, star1 };

        // 다이스 초기 면 — 6색 순서대로 할당 (None이면 매칭 불가)
        if (levelData.InitialDiceFaces == null || levelData.InitialDiceFaces.Length != 6)
            levelData.InitialDiceFaces = new RcColorType[6];
        for (int i = 0; i < 6; i++)
            levelData.InitialDiceFaces[i] = AllColors[i % AllColors.Length];

        return seed;
    }

    private static bool[] GenerateShape(GenParams p, System.Random rng, int w, int h)
    {
        int targetCount = Mathf.RoundToInt(w * h * Mathf.Clamp(p.FillRatio, 0.1f, 1f));
        targetCount = Mathf.Clamp(targetCount, p.ColorCount, w * h);

        return p.Preset switch
        {
            Preset.Full    => GenerateFull(rng, w, h, targetCount),
            Preset.Path    => GeneratePath(rng, w, h, targetCount),
            Preset.Cluster => GenerateCluster(rng, w, h, targetCount, p.ColorCount),
            _              => GenerateFull(rng, w, h, targetCount),
        };
    }

    private static bool[] GenerateFull(System.Random rng, int w, int h, int target)
    {
        bool[] active = new bool[w * h];
        for (int i = 0; i < active.Length; i++) active[i] = true;

        var indices = Enumerable.Range(0, active.Length).ToList();
        Shuffle(indices, rng);
        int remove = active.Length - target;
        for (int i = 0; i < remove; i++) active[indices[i]] = false;

        return active;
    }

    private static bool[] GeneratePath(System.Random rng, int w, int h, int target)
    {
        bool[] active  = new bool[w * h];
        int    cur     = (h / 2) * w + (w / 2);
        active[cur]    = true;
        int placed     = 1;
        int maxIter    = w * h * 20;
        int[] dirs     = { -1, 1, -w, w };

        for (int iter = 0; iter < maxIter && placed < target; iter++)
        {
            int dir  = dirs[rng.Next(dirs.Length)];
            int next = cur + dir;
            if (next < 0 || next >= w * h) continue;

            // 좌우 이동 시 행 경계 넘기 방지
            if (Math.Abs((next % w) - (cur % w)) > 1) continue;

            cur = next;
            if (!active[cur]) { active[cur] = true; placed++; }
        }

        return active;
    }

    private static bool[] GenerateCluster(System.Random rng, int w, int h, int target, int colorCount)
    {
        bool[] active   = new bool[w * h];
        var    frontier = new List<int>();
        int    clusters = Mathf.Max(2, Mathf.Min(colorCount, target / 3 + 1));

        var usedSeeds = new HashSet<int>();
        for (int i = 0; i < clusters; i++)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                int idx = rng.Next(w * h);
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
            if (cx > 0     && !active[cell - 1]) neighbors.Add(cell - 1);
            if (cx < w - 1 && !active[cell + 1]) neighbors.Add(cell + 1);
            if (cy > 0     && !active[cell - w]) neighbors.Add(cell - w);
            if (cy < h - 1 && !active[cell + w]) neighbors.Add(cell + w);

            if (neighbors.Count == 0) { frontier.RemoveAt(pick); continue; }

            int chosen = neighbors[rng.Next(neighbors.Count)];
            active[chosen] = true;
            frontier.Add(chosen);
            placed++;
        }

        return active;
    }

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
        var usedColors = AllColors.Take(colorCount).ToArray();

        var shuffled = cells.ToList();
        Shuffle(shuffled, rng);

        for (int i = 0; i < shuffled.Count; i++)
            ld.Tiles[shuffled[i]] = new RcTileData { TileType = tileType, colorType = usedColors[i % colorCount] };
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
