using UnityEngine;

/// 행동: 타일 클리어 처리 (기존 ColorMatchBehavior의 ClearTile 동작 대체)
[CreateAssetMenu(fileName = "ClearTileAction", menuName = "Rolice/Tile/Action/Clear Tile")]
public class RcClearTileActionSO : RcTileActionSO
{
    [Header("클리어 시 Material")]
    [Tooltip("클리어된 타일에 적용할 Material (옵션)")]
    public Material ClearedMaterial;

    public override void Execute(RcDicePawn pawn, RcTileData tileData)
    {
        if (tileData is not RcColorTileData colorTile) return;
        if (colorTile.IsCleared) return;
        colorTile.IsCleared = true;

        // Material 변경
        if (ClearedMaterial != null)
        {
            MeshRenderer renderer = tileData.TileObject.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
                renderer.material = ClearedMaterial;
        }

        // 이펙트 재생 (색상 SO의 MatchEffectPrefab 사용)
        var effectPrefab = colorTile.Color?.MatchEffectPrefab;
        if (effectPrefab != null)
        {
            GameObject effect = Object.Instantiate(
                effectPrefab,
                tileData.TileObject.transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );
            Object.Destroy(effect, 2f);
        }

        // LevelManager에 클리어 통보
        Vector2Int pos = RcMapGenerator.WorldToGrid(tileData.TileObject.transform.position);
        RcLevelManager.Instance.ClearColorTile(pos);
    }
}
