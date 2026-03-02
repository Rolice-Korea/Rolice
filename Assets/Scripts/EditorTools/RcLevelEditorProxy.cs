using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RcLevelEditorProxy : MonoBehaviour
{
    [Header("편집할 레벨")]
    public RcLevelDataSO LevelData;

    internal readonly Dictionary<Vector2Int, GameObject> SpawnedTiles = new();

    void OnEnable()
    {
        if (Application.isPlaying) return;
#if UNITY_EDITOR
        RebuildScene();
#endif
    }

    void OnDisable()
    {
        if (Application.isPlaying) return;
#if UNITY_EDITOR
        ClearScene();
#endif
    }

    // ── Scene 관리 ─────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    public void RebuildScene()
    {
        ClearScene();
        if (LevelData == null) return;

        for (int y = 0; y < LevelData.Height; y++)
        for (int x = 0; x < LevelData.Width;  x++)
            SpawnTileAt(new Vector2Int(x, y));
    }

    public void RefreshTileAt(Vector2Int cell)
    {
        DestroyTileAt(cell);
        SpawnTileAt(cell);
    }

    public void ClearScene()
    {
        foreach (var go in SpawnedTiles.Values)
            if (go != null) DestroyImmediate(go);
        SpawnedTiles.Clear();
    }

    // ── 내부 ───────────────────────────────────────────────────────────────────

    void SpawnTileAt(Vector2Int cell)
    {
        if (LevelData == null) return;

        var tileData = LevelData.GetTile(cell.x, cell.y);
        if (tileData == null || tileData.IsEmpty || tileData.TileType?.Prefab == null) return;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(tileData.TileType.Prefab);
        go.transform.SetParent(transform);
        go.transform.position = RcMapGenerator.GridToWorld(cell);
        go.name      = $"Tile_{cell.x}_{cell.y}";
        go.hideFlags = HideFlags.DontSave;
        SpawnedTiles[cell] = go;
        tileData.InitializeVisual(go);
    }

    void DestroyTileAt(Vector2Int cell)
    {
        if (!SpawnedTiles.TryGetValue(cell, out var old)) return;
        if (old != null) DestroyImmediate(old);
        SpawnedTiles.Remove(cell);
    }
#endif
}
