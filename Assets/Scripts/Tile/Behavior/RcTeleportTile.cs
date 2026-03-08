using UnityEngine;
using Rolice;

public class RcTeleportTile : RcTileBase
{
    private string myTeleportID;
    private string targetTeleportID;

    public override void Construct(RcTileData tileData, Vector2Int pos)
    {
        base.Construct(tileData, pos);
        bClearable = false;

        myTeleportID = tileData.TeleportTileID;
        targetTeleportID = tileData.TeleportTargetID;

        RcLevelManager.Instance?.RegisterTeleport(myTeleportID, gridPos);
    }

    public override void OnDiceEnter(RcDicePawn pawn)
    {
        if (string.IsNullOrEmpty(targetTeleportID)) return;

        var targetPos = RcLevelManager.Instance?.FindTeleportTarget(targetTeleportID);

        if (targetPos.HasValue)
        {
            pawn.Teleport(targetPos.Value);
        }
    }
}
