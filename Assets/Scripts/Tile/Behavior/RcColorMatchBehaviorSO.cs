using Cysharp.Threading.Tasks;
using UnityEngine;
using Rolice.Particle;

[CreateAssetMenu(fileName = "ColorMatchBehavior", menuName = "Rolice/Behaviors/Color Match")]
public class RcColorMatchBehaviorSO : RcTileBehaviorSO
{
    [Header("Visual Feedback")]
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
            ClearTile(pawn);
    }

    public void OnExit(RcDicePawn pawn)
    {
    }

    private void ClearTile(RcDicePawn pawn)
    {
        if (isCleared) return;

        isCleared = true;

        // 마지막 타일이면 승리 이미션이 바로 뒤따르므로 색 매칭 이미션 스킵
        bool isLastTile = RcLevelManager.Instance.GetRemainingColorTiles() == 1;
        ApplyVisualFeedback(pawn, skipEmission: isLastTile);
        RcLevelManager.Instance.ClearColorTile(tilePosition);
    }

    private void ApplyVisualFeedback(RcDicePawn pawn, bool skipEmission = false)
    {
        if (settings.clearedMaterial != null && tileRenderer != null)
            tileRenderer.material = settings.clearedMaterial;

        if (!skipEmission)
            pawn.FlashEmission();

        var prefab = tileData.Color?.MatchEffectPrefab;
        if (prefab == null) return;

        var go = Object.Instantiate(
            prefab,
            tileObject.transform.position + Vector3.up * 0.7f,
            Quaternion.identity
        );

        var effect = go.GetComponent<RcParticleEffect>();
        if (effect != null)
        {
            effect.OnCompleted += () => Object.Destroy(go);
            effect.PlayAsync().Forget();
        }
        else
        {
            Object.Destroy(go, 2f);
        }
    }
}
