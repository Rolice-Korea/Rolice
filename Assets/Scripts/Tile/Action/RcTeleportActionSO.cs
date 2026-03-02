using UnityEngine;

/// 행동: 텔레포트 실행 (기존 RcTeleportBehavior 대체)
[CreateAssetMenu(fileName = "TeleportAction", menuName = "Rolice/Tile/Action/Teleport")]
public class RcTeleportActionSO : RcTileActionSO
{
    [Header("Visual & Audio")]
    [Tooltip("텔레포트 시작 시 재생할 이펙트")]
    public GameObject TeleportOutEffectPrefab;

    [Tooltip("텔레포트 도착 시 재생할 이펙트")]
    public GameObject TeleportInEffectPrefab;

    [Tooltip("텔레포트 사운드")]
    public AudioClip TeleportSound;

    [Tooltip("텔레포트 애니메이션 지속 시간")]
    public float TeleportDuration = 0.5f;

    /// 타일 생성 시 텔레포트 페어 등록
    public override void Initialize(RcTileData tileData)
    {
        if (tileData is not RcTeleportTileData teleportTile)
        {
            Debug.LogWarning("[TeleportAction] RcTeleportTileData가 아닌 타일에 적용되었습니다.");
            return;
        }

        Vector2Int pos = RcMapGenerator.WorldToGrid(tileData.TileObject.transform.position);
        RcLevelManager.Instance.RegisterTeleportPair(teleportTile.PairID, pos);
    }

    public override void Execute(RcDicePawn pawn, RcTileData tileData)
    {
        if (tileData is not RcTeleportTileData teleportTile)
        {
            Debug.LogWarning("[TeleportAction] RcTeleportTileData가 아닌 타일에 적용되었습니다.");
            return;
        }

        Vector2Int tilePos    = RcMapGenerator.WorldToGrid(tileData.TileObject.transform.position);
        Vector2Int? targetPos = RcLevelManager.Instance.FindTeleportPair(teleportTile.PairID, tilePos);

        if (!targetPos.HasValue)
        {
            Debug.LogWarning($"[TeleportAction] 페어 타일을 찾을 수 없습니다: {teleportTile.PairID}");
            return;
        }

        ExecuteTeleport(pawn, tileData, targetPos.Value);
    }

    private void ExecuteTeleport(RcDicePawn pawn, RcTileData tileData, Vector2Int targetPos)
    {
        Vector3 fromPos = tileData.TileObject.transform.position;

        PlayEffect(fromPos, TeleportOutEffectPrefab);

        if (TeleportSound != null)
            AudioSource.PlayClipAtPoint(TeleportSound, fromPos);

        Vector3 targetWorldPos = RcMapGenerator.GridToWorld(targetPos);
        PlayEffect(targetWorldPos, TeleportInEffectPrefab);

        pawn.Teleport(targetPos, TeleportDuration);
    }

    private void PlayEffect(Vector3 position, GameObject effectPrefab)
    {
        if (effectPrefab == null) return;

        GameObject effect = Object.Instantiate(
            effectPrefab,
            position + Vector3.up * 0.5f,
            Quaternion.identity
        );
        Object.Destroy(effect, 3f);
    }
}
