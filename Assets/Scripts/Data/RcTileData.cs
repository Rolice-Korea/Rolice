using System;
using UnityEngine;
using Rolice;

[Serializable]
public class RcTileData
{
    [Header("Tile Type")]
    public RcTileTypeSO TileType;

    public RcColorType colorType;
    public int StoneMaxHits = 1;
    public Vector2Int TeleportTargetGrid;

    public bool IsEmpty => TileType == null;
}
