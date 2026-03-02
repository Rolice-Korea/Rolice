using System;
using UnityEngine;

/// 색상 매칭이 필요한 타일의 데이터.
/// Color 필드를 가지며, ColorMatchCondition / RandomizeColorAction에서 사용된다.
[Serializable]
public class RcColorTileData : RcTileData
{
    [Header("Color")]
    [Tooltip("이 타일의 색상. 주사위 바닥면 색상과 비교된다.")]
    public RcColorSO Color;

    [NonSerialized] public bool IsCleared;

    public override void InitializeVisual(GameObject tileObject)
    {
        if (Color?.TileMaterial == null) return;
        var rend = tileObject.GetComponentInChildren<Renderer>();
        if (rend != null) rend.sharedMaterial = Color.TileMaterial;
    }

    public override RcTileData Clone()
    {
        return new RcColorTileData
        {
            TileType = this.TileType,
            bCanEnter = this.bCanEnter,
            Color = this.Color
        };
    }
}
