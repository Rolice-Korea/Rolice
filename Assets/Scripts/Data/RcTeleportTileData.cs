using System;

[Serializable]
public class RcTeleportTileData : RcTileData
{
    [UnityEngine.Tooltip("같은 ID를 가진 다른 타일과 페어를 형성합니다")]
    public string PairID = "TP_01";

    public override RcTileData Clone()
    {
        return new RcTeleportTileData
        {
            TileType  = this.TileType,
            bCanEnter = this.bCanEnter,
            PairID    = this.PairID
        };
    }
}
