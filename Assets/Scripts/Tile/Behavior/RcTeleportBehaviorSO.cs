using UnityEngine;

[CreateAssetMenu(fileName = "TeleportBehavior", menuName = "Rolice/Behaviors/Teleport")]
public class RcTeleportBehaviorSO : RcTileBehaviorSO
{
    [Header("Teleport Settings")]
    [Tooltip("같은 ID를 가진 다른 타일과 페어를 형성합니다")]
    public string pairID = "TP_01";

    [Header("Visual & Audio")]
    [Tooltip("텔레포트 시작 시 재생할 이펙트")]
    public GameObject teleportOutEffectPrefab;

    [Tooltip("텔레포트 도착 시 재생할 이펙트")]
    public GameObject teleportInEffectPrefab;

    [Tooltip("텔레포트 사운드")]
    public AudioClip teleportSound;

    [Tooltip("텔레포트 애니메이션 지속 시간")]
    public float teleportDuration = 0.5f;

    public override ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData)
    {
        return new RcTeleportBehavior(this, tileObject, tileData);
    }
}

public class RcTeleportBehavior : ITileBehavior
{
    private readonly RcTeleportBehaviorSO settings;
    private readonly GameObject tileObject;
    private readonly Vector2Int tilePosition;

    public RcTeleportBehavior(RcTeleportBehaviorSO settings, GameObject tileObject, RcTileData tileData)
    {
        this.settings = settings;
        this.tileObject = tileObject;
        this.tilePosition = RcMapGenerator.WorldToGrid(tileObject.transform.position);

        RcLevelManager.Instance.RegisterTeleportPair(settings.pairID, tilePosition);
    }

    public bool CanEnter(RcDicePawn pawn)
    {
        return true;
    }

    public void OnEnter(RcDicePawn pawn)
    {
        Vector2Int? targetPos = RcLevelManager.Instance.FindTeleportPair(settings.pairID, tilePosition);

        if (!targetPos.HasValue)
        {
            Debug.LogWarning($"[TeleportBehavior] 페어 타일을 찾을 수 없습니다: {settings.pairID}");
            return;
        }

        ExecuteTeleport(pawn, targetPos.Value);
    }

    public void OnExit(RcDicePawn pawn)
    {
    }

    private void ExecuteTeleport(RcDicePawn pawn, Vector2Int targetPos)
    {
        PlayEffect(tileObject.transform.position, settings.teleportOutEffectPrefab);

        if (settings.teleportSound != null)
            AudioSource.PlayClipAtPoint(settings.teleportSound, tileObject.transform.position);

        Vector3 targetWorldPos = RcMapGenerator.GridToWorld(targetPos);
        PlayEffect(targetWorldPos, settings.teleportInEffectPrefab);

        pawn.Teleport(targetPos, settings.teleportDuration);
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
