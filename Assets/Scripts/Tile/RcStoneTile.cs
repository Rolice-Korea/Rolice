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
        targetColor = tileData.colorType;
        maxHits = tileData.StoneMaxHits;
        bClear = false;
    }

    public override void OnDiceEnter(RcDicePawn dice)
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
        var flatData = new RcTileData { colorType = targetColor };
        colorTile.Construct(flatData, gridPos);
        RcLevelManager.Instance?.UpdateTile(gridPos, colorTile);
        Destroy(this);
    }
}
