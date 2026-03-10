using UnityEngine;
using Rolice;

public class RcTeleportTile : RcTileBase
{
    private Vector2Int targetGrid;

    public override void Construct(RcTileData tileData, Vector2Int pos)
    {
        base.Construct(tileData, pos);
        bClearable = false;

        targetGrid = tileData.TeleportTargetGrid;
    }

    public override void OnDiceEnter(RcDicePawn dice)
    {
        dice.Teleport(targetGrid);
    }
}
