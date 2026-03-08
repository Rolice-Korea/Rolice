using UnityEngine;
using Rolice;

public class RcTeleportTile : RcTileBase
{
    private string myTeleportID;
    private string targetTeleportID;

    public override void Construct(RcTileData tileData)
    {
        bClearable = false;

        myTeleportID = tileData.TeleportTileID;
        targetTeleportID = tileData.TeleportTargetID;
        
        var pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        RcLevelManager.Instance?.RegisterTeleport(myTeleportID, pos);
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
