using UnityEngine;
using Rolice;

public class RcColorTile : RcTileBase
{
    private RcColorType color;

    public override void Construct(RcTileData tileData)
    {
        color = tileData.Color;
        bClear = false;
    }

    public override void OnDiceEnter(RcDicePawn pawn)
    {
        if (bClear) return;
        
        if (pawn.GetBottomColor() == color)
        {
            Clear();
            
            if (RcDataTableManager.Instance != null)
            {
                var faceData = RcDataTableManager.FaceDataTable.GetFaceData(RcFaceSkinType.Default);
                var fxPrefab = faceData.GetMatchEffect(color);
                if (fxPrefab != null)
                {
                    Instantiate(fxPrefab, transform.position, Quaternion.identity);
                }
            }
        }
    }
}
