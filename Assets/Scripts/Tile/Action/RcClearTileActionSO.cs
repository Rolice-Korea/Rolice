using UnityEngine;

/// 행동: 타일 클리어 처리 (기존 ColorMatchBehavior의 ClearTile 동작 대체)
[CreateAssetMenu(fileName = "ClearTileAction", menuName = "Rolice/Tile/Action/Clear Tile")]
public class RcClearTileActionSO : RcTileActionSO
{
    [Header("클리어 시 Material")]
    [Tooltip("클리어된 타일에 적용할 Material (옵션)")]
    public Material ClearedMaterial;

    [Header("클리어 이펙트")]
    [Tooltip("클리어 시 재생할 파티클 프리팹 (옵션)")]
    public GameObject ClearEffectPrefab;

    public override void Execute(RcDicePawn pawn, RcTileData tileData)
    {
        // Material 변경
        if (ClearedMaterial != null)
        {
            MeshRenderer renderer = tileData.TileObject.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
                renderer.material = ClearedMaterial;
        }

        // 이펙트 재생
        if (ClearEffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(
                ClearEffectPrefab,
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
