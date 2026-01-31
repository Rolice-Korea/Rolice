using UnityEngine;

/// <summary>
/// 색깔 매칭 타일 행동 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ColorMatchBehavior", menuName = "Rolice/Behaviors/Color Match")]
public class RcColorMatchBehaviorSO : RcTileBehaviorSO
{
    [Header("Visual Feedback")]
    [Tooltip("색깔이 매칭되었을 때 재생할 파티클")]
    public GameObject matchEffectPrefab;
    
    [Tooltip("클리어된 타일의 Material")]
    public Material clearedMaterial;
    
    public override ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData)
    {
        return new RcColorMatchBehavior(this, tileObject, tileData);
    }
}

/// <summary>
/// 색깔 매칭 타일의 런타임 행동
/// - 주사위의 바닥 면 색깔과 타일 색깔이 일치하면 클리어
/// - LevelManager에게 클리어 상태를 알림
/// </summary>
public class RcColorMatchBehavior : ITileBehavior
{
    private RcColorMatchBehaviorSO settings;
    private GameObject tileObject;
    private RcTileData tileData;
    private MeshRenderer tileRenderer;
    
    private bool isCleared;
    private Vector2Int tilePosition;
    
    public RcColorMatchBehavior(RcColorMatchBehaviorSO settings, GameObject tileObject, RcTileData tileData)
    {
        this.settings = settings;
        this.tileObject = tileObject;
        this.tileData = tileData;
        this.tileRenderer = tileObject.GetComponent<MeshRenderer>();
        
        // 타일 위치 계산
        this.tilePosition = RcMapGenerator.WorldToGrid(tileObject.transform.position);
        
        this.isCleared = false;
    }
    
    public bool CanEnter(RcDicePawn pawn)
    {
        // 색깔 타일은 항상 진입 가능
        return true;
    }
    
    public void OnEnter(RcDicePawn pawn)
    {
        // 이미 클리어된 타일은 다시 체크하지 않음
        if (isCleared) return;
        
        // 타일 색깔 가져오기
        var materialDB = RcDataManager.Instance.MaterialDatabase;
        ColorType? tileColor = materialDB.GetColorType(tileRenderer.sharedMaterial);
        
        if (!tileColor.HasValue)
        {
            Debug.LogWarning($"[ColorMatchBehavior] 타일 색깔을 찾을 수 없습니다: {tileObject.name}");
            return;
        }
        
        // 주사위 바닥 면 색깔과 비교
        ColorType diceBottomColor = pawn.GetBottomColor();
        
        if (diceBottomColor == tileColor.Value)
        {
            ClearTile();
        }
    }
    
    public void OnExit(RcDicePawn pawn)
    {
        // 색깔 타일은 나갈 때 특별한 동작 없음
    }
    
    /// <summary>
    /// 타일을 클리어 처리합니다
    /// </summary>
    private void ClearTile()
    {
        if (isCleared) return;
        
        isCleared = true;
        
        // 1. 비주얼 피드백
        ApplyVisualFeedback();
        
        // 2. 레벨 매니저에게 클리어 알림
        RcLevelManager.Instance.ClearColorTile(tilePosition);
        
        Debug.Log($"[ColorMatchBehavior] 타일 클리어: {tileObject.name} at {tilePosition}");
    }
    
    /// <summary>
    /// 클리어 시각 효과를 적용합니다
    /// </summary>
    private void ApplyVisualFeedback()
    {
        // Material 변경
        if (settings.clearedMaterial != null && tileRenderer != null)
        {
            tileRenderer.material = settings.clearedMaterial;
        }
        
        // 파티클 효과
        if (settings.matchEffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(
                settings.matchEffectPrefab, 
                tileObject.transform.position + Vector3.up * 0.5f, 
                Quaternion.identity
            );
            Object.Destroy(effect, 2f);
        }
    }
}