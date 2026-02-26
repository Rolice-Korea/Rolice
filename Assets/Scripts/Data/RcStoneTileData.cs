using System;
using UnityEngine;

/// 색 매칭 시 여러 번 맞춰야 클리어되는 타일.
/// HitCount는 런타임 전용 — 직렬화 안 됨, 항상 0에서 시작.
[Serializable]
public class RcStoneTileData : RcColorTileData
{
    [NonSerialized] public int HitCount;

    public override RcTileData Clone()
    {
        return new RcStoneTileData
        {
            TileType = this.TileType,
            bCanEnter = this.bCanEnter,
            Color = this.Color
            // HitCount는 NonSerialized — 클론 시 0으로 초기화됨
        };
    }
}
