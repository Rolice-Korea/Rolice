using UnityEngine;
using Rolice;

public class RcStoneTile : RcTileBase
{
    private RcColorType targetColor;
    private int currentHits = 0;
    private int maxHits = 1;

    public override void Construct(RcTileData tileData)
    {
        targetColor = tileData.Color;
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
        var colorTile = gameObject.AddComponent<RcColorTile>();
        var flatData = new RcTileData { Color = targetColor };
        colorTile.Construct(flatData);
        Destroy(this);
    }
}
