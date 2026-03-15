using UnityEngine;
using Rolice;

public class RcNormalTile : RcTileBase
{
    [SerializeField] private Renderer tileRenderer;
    [SerializeField] private Material clearedMaterial;

    private RcColorType color;

    public override void Construct(RcTileData tileData, Vector2Int pos)
    {
        base.Construct(tileData, pos);
        color = tileData.colorType;
        bClear = false;
        ApplyColorMaterial();
    }

    private void ApplyColorMaterial()
    {
        if (tileRenderer == null) return;
        var mat = RcDataTableManager.FaceDataTable?.GetFaceData(RcFaceSkinType.Default)?.GetTileMaterial(color);
        if (mat != null)
            tileRenderer.material = mat;
    }

    public override void OnCleared()
    {
        if (tileRenderer != null && clearedMaterial != null)
            tileRenderer.material = clearedMaterial;
    }

    public override void OnDiceEnter(RcDicePawn dice)
    {
        if (bClear) return;

        if (dice.GetBottomColor() == color)
        {
            Clear();

            if (RcDataTableManager.Instance != null)
            {
                var faceData = RcDataTableManager.FaceDataTable.GetFaceData(RcFaceSkinType.Default);
                var fxPrefab = faceData?.GetMatchEffect(color);
                if (fxPrefab != null)
                {
                    Instantiate(fxPrefab, transform.position, Quaternion.identity);
                }
            }
        }
    }
}
