using System;
using System.Collections.Generic;
using Engine;
using UnityEngine;
using Object = UnityEngine.Object;

public class RcLevelManager : RcSingleton<RcLevelManager>
{
    private RcLevelDataSO currentLevelData;
    private Dictionary<Vector2Int, RcTileBase> runtimeTiles;
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
            foreach (var tile in runtimeTiles.Values)
            {
                if (tile != null)
                    Object.Destroy(tile.gameObject);
            }
        }

        runtimeTiles?.Clear();
        colorTilesRemaining?.Clear();
        teleportManager?.Clear();

        currentLevelData = null;
        IsInitialized = false;
    }

    private void InitializeCollections()
    {
        runtimeTiles = new Dictionary<Vector2Int, RcTileBase>();
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

                GameObject tileObj = SpawnTile(sourceTile.TileType, gridPos, tilesParent);
                if (tileObj == null)
                {
                    Debug.LogError($"[LevelManager] 타일 생성 실패: {sourceTile.TileType.name} at ({x}, {y})");
                    continue;
                }

                RcTileBase runtimeTile = tileObj.GetComponent<RcTileBase>();
                if (runtimeTile != null)
                {
                    runtimeTile.Construct(sourceTile);
                    runtimeTiles[gridPos] = runtimeTile;

                    if (runtimeTile.IsClearable() && !runtimeTile.IsClear())
                    {
                        colorTilesRemaining.Add(gridPos);
                    }
                }

                tilesCreated++;
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

    public RcTileBase GetTile(Vector2Int pos)
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

    public void RegisterTeleport(string tileID, Vector2Int position)
    {
        teleportManager?.Register(tileID, position);
    }

    public Vector2Int? FindTeleportTarget(string targetID)
    {
        return teleportManager?.FindTarget(targetID);
    }

    public int GetRemainingColorTiles()
    {
        return colorTilesRemaining?.Count ?? 0;
    }
}
