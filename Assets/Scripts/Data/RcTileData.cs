using System;
using UnityEngine;
using Rolice;

[Serializable]
public class RcTileData
{
    [Header("Tile Type")]
    public RcTileTypeSO TileType;

    public RcColorType Color;
    public string TeleportTileID;
    public string TeleportTargetID;

    public bool IsEmpty => TileType == null;
}
