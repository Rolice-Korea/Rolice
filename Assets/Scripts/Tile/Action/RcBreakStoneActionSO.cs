using UnityEngine;

/// 행동: 스톤 감소 (히트 카운트 증가) + 단계별 Material 변경 + 이펙트 재생
[CreateAssetMenu(fileName = "BreakStoneAction", menuName = "Rolice/Tile/Action/Break Stone")]
public class RcBreakStoneActionSO : RcTileActionSO
{
    [Header("단계별 Material")]
    [Tooltip("인덱스 0 = 1번째 히트, 1 = 2번째 히트... 배열 크기 = 최대 히트 횟수")]
    public Material[] HitMaterials;

    [Header("이펙트")]
    [Tooltip("히트 시 재생할 파티클 프리팹 (옵션)")]
    public GameObject BreakEffectPrefab;

    public override void Execute(RcDicePawn pawn, RcTileData tileData)
    {
        if (tileData is not RcStoneTileData stoneTile)
        {
            Debug.LogWarning("[BreakStoneAction] RcStoneTileData가 아닌 타일에 적용되었습니다.");
            return;
        }

        stoneTile.HitCount++;

        // 단계별 Material 변경
        ApplyHitMaterial(tileData, stoneTile.HitCount);

        // 이펙트 재생
        PlayBreakEffect(tileData);

        // 이벤트 발행
        Vector2Int pos = RcMapGenerator.WorldToGrid(tileData.TileObject.transform.position);
        RcGameEvents.Instance.Publish(RcGameEvent.BreakableTileHit, pos);
    }

    private void ApplyHitMaterial(RcTileData tileData, int hitCount)
    {
        if (HitMaterials == null || HitMaterials.Length == 0) return;

        MeshRenderer renderer = tileData.TileObject.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return;

        int materialIndex = Mathf.Clamp(hitCount - 1, 0, HitMaterials.Length - 1);
        if (HitMaterials[materialIndex] != null)
            renderer.material = HitMaterials[materialIndex];
    }

    private void PlayBreakEffect(RcTileData tileData)
    {
        if (BreakEffectPrefab == null) return;

        GameObject effect = Object.Instantiate(
            BreakEffectPrefab,
            tileData.TileObject.transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );
        Object.Destroy(effect, 2f);
    }
}
