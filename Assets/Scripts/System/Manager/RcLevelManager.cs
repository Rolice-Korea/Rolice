using System;
using System.Collections.Generic;
using Engine;
using UnityEngine;
using Object = UnityEngine.Object;

public class RcLevelManager : RcSingleton<RcLevelManager>
{
    private RcLevelDataSO currentLevelData;
    private Dictionary<Vector2Int, RcTileData> runtimeTiles;
    private HashSet<Vector2Int> colorTilesRemaining;
    private RcTeleportPairManager teleportManager;

    public bool IsInitialized { get; private set; }

    public RcLevelLoadResult LoadLevel(RcLevelDataSO levelData, Transform tilesParent = null)
    {
        if (levelData == null)
            return RcLevelLoadResult.CreateFailure("LevelData가 null입니다");

        if (levelData.Width <= 0 || levelData.Height <= 0)
            return RcLevelLoadResult.CreateFailure($"잘못된 맵 크기: {levelData.Width}x{levelData.Height}");

        try
        {
            ClearLevel();
            InitializeCollections();
            currentLevelData = levelData;

            int tilesCreated = GenerateMap(tilesParent);
            IsInitialized = true;

            return RcLevelLoadResult.CreateSuccess(tilesCreated, colorTilesRemaining.Count);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelManager] 레벨 로드 실패: {e.Message}");
            ClearLevel();
            return RcLevelLoadResult.CreateFailure(e.Message);
        }
    }

    public void ClearLevel()
    {
        if (runtimeTiles != null)
        {
            foreach (var tileData in runtimeTiles.Values)
                Object.Destroy(tileData.TileObject);
        }

        runtimeTiles?.Clear();
        colorTilesRemaining?.Clear();
        teleportManager?.Clear();

        currentLevelData = null;
        IsInitialized = false;
    }

    private void InitializeCollections()
    {
        runtimeTiles = new Dictionary<Vector2Int, RcTileData>();
        colorTilesRemaining = new HashSet<Vector2Int>();
        teleportManager = new RcTeleportPairManager();
    }

    private int GenerateMap(Transform tilesParent)
    {
        int tilesCreated = 0;

        for (int y = 0; y < currentLevelData.Height; y++)
        {
            for (int x = 0; x < currentLevelData.Width; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                RcTileData sourceTile = currentLevelData.GetTile(x, y);

                if (sourceTile == null || sourceTile.IsEmpty)
                    continue;

                RcTileData runtimeTile = sourceTile.Clone();
                runtimeTiles[gridPos] = runtimeTile;

                GameObject tileObj = SpawnTile(sourceTile.TileType, gridPos, tilesParent);
                if (tileObj == null)
                {
                    Debug.LogError($"[LevelManager] 타일 생성 실패: {sourceTile.TileType.name} at ({x}, {y})");
                    continue;
                }

                // Runner를 동적으로 부착하고 TileTypeSO의 Rules로 구성
                RcTileRuleRunner runner = tileObj.AddComponent<RcTileRuleRunner>();
                runtimeTile.Setup(tileObj, runner);
                runner.Initialize(sourceTile.TileType.Rules, sourceTile.TileType.RequiresClearTracking, runtimeTile);

                tilesCreated++;

                if (runner.RequiresClearTracking)
                    colorTilesRemaining.Add(gridPos);
            }
        }

        return tilesCreated;
    }

    private GameObject SpawnTile(RcTileTypeSO tileType, Vector2Int gridPos, Transform parent)
    {
        if (tileType.Prefab == null)
        {
            Debug.LogError($"[LevelManager] TileType '{tileType.name}'에 Prefab이 없습니다");
            return null;
        }

        Vector3 worldPos = RcMapGenerator.GridToWorld(gridPos);
        GameObject tileObj = Object.Instantiate(tileType.Prefab, worldPos, Quaternion.identity, parent);
        tileObj.name = $"Tile_{gridPos.x}_{gridPos.y}_{tileType.name}";
        return tileObj;
    }

    public RcTileData GetRuntimeTile(Vector2Int pos)
    {
        return runtimeTiles != null && runtimeTiles.TryGetValue(pos, out var tile) ? tile : null;
    }

    public bool HasTile(Vector2Int pos)
    {
        return runtimeTiles != null && runtimeTiles.ContainsKey(pos);
    }

    public void ClearColorTile(Vector2Int pos)
    {
        if (!colorTilesRemaining.Remove(pos))
            return;

        RcGameEvents.Instance.Publish(RcGameEvent.ColorTileCleared, pos);

        if (CheckLevelComplete())
            HandleLevelComplete();
    }

    public bool CheckLevelComplete()
    {
        return colorTilesRemaining.Count == 0;
    }

    private void HandleLevelComplete()
    {
        RcGameEvents.Instance.Publish(RcGameEvent.LevelCompleted);
    }

    public void RegisterTeleportPair(string pairID, Vector2Int position)
    {
        teleportManager?.Register(pairID, position);
    }

    public Vector2Int? FindTeleportPair(string pairID, Vector2Int myPosition)
    {
        return teleportManager?.FindPair(pairID, myPosition);
    }

    public int GetRemainingColorTiles()
    {
        return colorTilesRemaining?.Count ?? 0;
    }
}
