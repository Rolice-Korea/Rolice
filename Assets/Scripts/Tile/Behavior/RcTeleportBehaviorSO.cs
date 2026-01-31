using UnityEngine;

/// <summary>
/// 간단한 1대1 텔레포트 타일
/// - pairID가 같은 타일끼리 양방향 텔레포트
/// </summary>
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

/// <summary>
/// 텔레포트 타일의 런타임 행동
/// </summary>
public class RcTeleportBehavior : ITileBehavior
{
    private RcTeleportBehaviorSO settings;
    private GameObject tileObject;
    private Vector2Int tilePosition;
    
    public RcTeleportBehavior(RcTeleportBehaviorSO settings, GameObject tileObject, RcTileData tileData)
    {
        this.settings = settings;
        this.tileObject = tileObject;
        this.tilePosition = RcMapGenerator.WorldToGrid(tileObject.transform.position);
        
        // 페어 등록
        RcLevelManager.Instance.RegisterTeleportPair(settings.pairID, tilePosition);
    }
    
    public bool CanEnter(RcDicePawn pawn)
    {
        return true;
    }
    
    public void OnEnter(RcDicePawn pawn)
    {
        // 페어 타일 찾기
        Vector2Int? targetPos = RcLevelManager.Instance.FindTeleportPair(settings.pairID, tilePosition);
        
        if (!targetPos.HasValue)
        {
            Debug.LogWarning($"[TeleportBehavior] 페어 타일을 찾을 수 없습니다: {settings.pairID}");
            return;
        }
        
        // 텔레포트 실행
        ExecuteTeleport(pawn, targetPos.Value);
    }
    
    public void OnExit(RcDicePawn pawn)
    {
        // 특별한 동작 없음
    }
    
    private void ExecuteTeleport(RcDicePawn pawn, Vector2Int targetPos)
    {
        // 시작 이펙트
        PlayEffect(tileObject.transform.position, settings.teleportOutEffectPrefab);
        
        // 사운드
        if (settings.teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(settings.teleportSound, tileObject.transform.position);
        }
        
        // 도착 이펙트
        Vector3 targetWorldPos = RcMapGenerator.GridToWorld(targetPos);
        PlayEffect(targetWorldPos, settings.teleportInEffectPrefab);
        
        // 주사위 텔레포트 (OnEnter 발동 안 함)
        pawn.Teleport(targetPos, settings.teleportDuration);
        
        Debug.Log($"[TeleportBehavior] 텔레포트: {tilePosition} → {targetPos}");
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