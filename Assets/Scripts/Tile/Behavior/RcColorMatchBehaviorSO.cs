using UnityEngine;

[CreateAssetMenu(fileName = "ColorMatchBehavior", menuName = "Rolice/Behaviors/Color Match")]
public class RcColorMatchBehaviorSO : RcTileBehaviorSO
{
    [Header("Visual Feedback")]
    [Tooltip("색깔이 매칭되었을 때 재생할 파티클")]
    public GameObject matchEffectPrefab;

    [Tooltip("클리어된 타일의 Material")]
    public Material clearedMaterial;

    private void OnEnable()
    {
        // 색깔 타일은 항상 클리어 추적이 필요함
        RequiresClearTracking = true;
    }

    public override ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData)
    {
        return new RcColorMatchBehavior(this, tileObject, tileData);
    }
}

public class RcColorMatchBehavior : ITileBehavior
{
    private RcColorMatchBehaviorSO _settings;
    private GameObject _tileObject;
    private RcTileData _tileData;
    private MeshRenderer _tileRenderer;

    private bool _isCleared;
    private Vector2Int _tilePosition;

    public RcColorMatchBehavior(RcColorMatchBehaviorSO settings, GameObject tileObject, RcTileData tileData)
    {
        _settings = settings;
        _tileObject = tileObject;
        _tileData = tileData;
        _tileRenderer = tileObject.GetComponent<MeshRenderer>();
        _tilePosition = RcMapGenerator.WorldToGrid(tileObject.transform.position);
        _isCleared = false;
    }

    public bool CanEnter(RcDicePawn pawn)
    {
        return true;
    }

    public void OnEnter(RcDicePawn pawn)
    {
        if (_isCleared)
        {
            Debug.Log($"[ColorMatchBehavior] 이미 클리어된 타일: {_tileObject.name}");
            return;
        }

        RcColorSO tileColor = _tileData.Color;

        if (tileColor == null)
        {
            Debug.LogWarning($"[ColorMatchBehavior] 타일에 Color가 할당되지 않았습니다: {_tileObject.name}");
            return;
        }

        RcColorSO diceBottomColor = pawn.GetBottomColor();

        Debug.Log($"[ColorMatchBehavior] 색깔 비교: 타일={tileColor.DisplayName}, 주사위={diceBottomColor?.DisplayName}");

        if (diceBottomColor == tileColor)
        {
            Debug.Log($"[ColorMatchBehavior] ✓ 색깔 매칭! 타일 클리어 진행");
            ClearTile();
        }
        else
        {
            Debug.Log($"[ColorMatchBehavior] ✗ 색깔 불일치 (타일: {tileColor.DisplayName} vs 주사위: {diceBottomColor?.DisplayName})");
        }
    }

    public void OnExit(RcDicePawn pawn)
    {
    }

    private void ClearTile()
    {
        if (_isCleared) return;

        _isCleared = true;

        ApplyVisualFeedback();

        RcLevelManager.Instance.ClearColorTile(_tilePosition);

        Debug.Log($"[ColorMatchBehavior] 타일 클리어 완료: {_tileObject.name} at {_tilePosition}");
    }

    private void ApplyVisualFeedback()
    {
        if (_settings.clearedMaterial != null && _tileRenderer != null)
        {
            _tileRenderer.material = _settings.clearedMaterial;
        }

        if (_settings.matchEffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(
                _settings.matchEffectPrefab,
                _tileObject.transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );
            Object.Destroy(effect, 2f);
        }
    }
}
