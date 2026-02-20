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
        RequiresClearTracking = true;
    }

    public override ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData)
    {
        return new RcColorMatchBehavior(this, tileObject, tileData);
    }
}

public class RcColorMatchBehavior : ITileBehavior
{
    private readonly RcColorMatchBehaviorSO settings;
    private readonly GameObject tileObject;
    private readonly RcTileData tileData;
    private readonly MeshRenderer tileRenderer;
    private readonly Vector2Int tilePosition;

    private bool isCleared;

    public RcColorMatchBehavior(RcColorMatchBehaviorSO settings, GameObject tileObject, RcTileData tileData)
    {
        this.settings = settings;
        this.tileObject = tileObject;
        this.tileData = tileData;
        tileRenderer = tileObject.GetComponentInChildren<MeshRenderer>();
        tilePosition = RcMapGenerator.WorldToGrid(tileObject.transform.position);
        isCleared = false;
    }

    public bool CanEnter(RcDicePawn pawn)
    {
        return true;
    }

    public void OnEnter(RcDicePawn pawn)
    {
        if (isCleared) return;

        RcColorSO tileColor = this.tileData.Color;
        if (tileColor == null)
        {
            Debug.LogWarning($"[ColorMatchBehavior] 타일에 Color가 할당되지 않았습니다: {this.tileObject.name}");
            return;
        }

        RcColorSO diceBottomColor = pawn.GetBottomColor();

        if (diceBottomColor == tileColor)
            ClearTile();
    }

    public void OnExit(RcDicePawn pawn)
    {
    }

    private void ClearTile()
    {
        if (isCleared) return;

        isCleared = true;
        ApplyVisualFeedback();
        RcLevelManager.Instance.ClearColorTile(tilePosition);
    }

    private void ApplyVisualFeedback()
    {
        if (settings.clearedMaterial != null && tileRenderer != null)
            tileRenderer.material = settings.clearedMaterial;

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
