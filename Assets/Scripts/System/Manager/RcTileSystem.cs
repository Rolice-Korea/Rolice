using System.Collections.Generic;
using UnityEngine;
using Engine;

public class RcTileSystem : RcSingletonMono<RcTileSystem>
{
    public void Initialize(IEnumerable<RcTileBase> allTiles)
    {
        // 텔레포트가 아닌 클리어 대상 타일들만 집계 등 추가 가능 (현재는 더미)
    }
}
