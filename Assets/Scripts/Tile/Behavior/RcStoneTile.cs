using UnityEngine;
using Rolice;

public class RcStoneTile : RcTileBase
{
    private RcColorType targetColor;
    private int currentHits = 0;
    private int maxHits = 1;

    public override void Construct(RcTileData tileData, Vector2Int pos)
    {
        base.Construct(tileData, pos);
        targetColor = tileData.Color;
        maxHits = tileData.StoneMaxHits;
        bClear = false;
    }

    public override void OnDiceEnter(RcDicePawn pawn)
    {
        currentHits++;
        if (currentHits >= maxHits)
        {
            ConvertToColorTile();
        }
    }

    private void ConvertToColorTile()
    {
        var colorTile = gameObject.AddComponent<RcNormalTile>();
        var flatData = new RcTileData { Color = targetColor };
        colorTile.Construct(flatData, gridPos);
        RcLevelManager.Instance?.UpdateTile(gridPos, colorTile);
        Destroy(this);
    }
}
