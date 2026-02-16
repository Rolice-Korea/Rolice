using UnityEngine;

public static class RcMapGenerator
{
    private const string TILE_PREFAB_PATH = "Tiles/";

    public static GameObject CreateTile(string tileID, Vector2Int gridPos, Transform parent = null)
    {
        GameObject prefab = LoadTilePrefab(tileID);

        if (prefab == null)
        {
            Debug.LogError($"[MapGenerator] 타일 프리팹을 찾을 수 없습니다: {tileID}");
            return null;
        }

        Vector3 worldPos = GridToWorld(gridPos);
        GameObject tileObj = Object.Instantiate(prefab, worldPos, Quaternion.identity, parent);
        tileObj.name = $"Tile_{gridPos.x}_{gridPos.y}_{tileID}";

        return tileObj;
    }

    public static GameObject LoadTilePrefab(string tileID)
    {
        return Resources.Load<GameObject>(TILE_PREFAB_PATH + tileID);
    }

    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, 0f, gridPos.y);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.z)
        );
    }

    public static bool ValidateTilePrefab(string tileID)
    {
        return LoadTilePrefab(tileID) != null;
    }
}
