using System.Collections.Generic;
using UnityEngine;

/// 텔레포트 타일 페어를 단방향 관점(TileID -> Position)으로 관리하는 매니저
public class RcTeleportPairManager
{
    private Dictionary<string, Vector2Int> tilePositions = new Dictionary<string, Vector2Int>();
    
    public void Register(string tileID, Vector2Int position)
    {
        if (string.IsNullOrEmpty(tileID))
            return;

        tilePositions[tileID] = position;
    }
    
    public Vector2Int? FindTarget(string targetID)
    {
        if (string.IsNullOrEmpty(targetID))
            return null;

        if (tilePositions.TryGetValue(targetID, out var pos))
            return pos;

        return null;
    }
    
    public void Clear()
    {
        tilePositions.Clear();
    }
}
