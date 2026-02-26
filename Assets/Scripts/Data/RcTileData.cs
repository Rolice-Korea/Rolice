using System;
using UnityEngine;

[Serializable]
public class RcTileData
{
    [Header("Basic Info")]
    public string TileID;

    public bool bCanEnter = true;

    [Header("Color (색깔 타일용)")]
    public RcColorSO Color;

    [Header("Runtime State")]
    [NonSerialized] public int StoneCount;

    public GameObject TileObject;

    public void Setup(GameObject tileObject)
    {
        this.TileObject = tileObject;
    }

    public bool CanEnter(RcDicePawn pawn)
    {
        return bCanEnter;
    }

    public RcTileData Clone()
    {
        return new RcTileData
        {
            TileID = this.TileID,
            bCanEnter = this.bCanEnter,
            Color = this.Color
        };
    }
}
