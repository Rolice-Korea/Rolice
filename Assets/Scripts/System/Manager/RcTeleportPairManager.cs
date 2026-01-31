using System.Collections.Generic;
using UnityEngine;

/// 텔레포트 타일 페어를 관리하는 전용 클래스
/// - LevelManager의 책임 분리
/// - 텔레포트 관련 로직 캡슐화
public class RcTeleportPairManager
{
    private Dictionary<string, List<Vector2Int>> pairs = new Dictionary<string, List<Vector2Int>>();
    
    public void Register(string pairID, Vector2Int position)
    {
        if (string.IsNullOrEmpty(pairID))
        {
            Debug.LogWarning("[TeleportPairManager] PairID가 비어있습니다!");
            return;
        }

        if (!pairs.ContainsKey(pairID))
        {
            pairs[pairID] = new List<Vector2Int>();
        }

        if (!pairs[pairID].Contains(position))
        {
            pairs[pairID].Add(position);
            Debug.Log($"[TeleportPairManager] 텔레포트 페어 등록: {pairID} at {position}");
        }
    }
    
    public Vector2Int? FindPair(string pairID, Vector2Int myPosition)
    {
        if (!pairs.TryGetValue(pairID, out var positions))
            return null;

        // 자기 자신을 제외한 첫 번째 타일 반환
        foreach (var pos in positions)
        {
            if (pos != myPosition)
                return pos;
        }

        return null;
    }
    
    public void Clear()
    {
        pairs.Clear();
    }
    
    public int GetPairCount()
    {
        return pairs.Count;
    }
    
    public int GetTileCount(string pairID)
    {
        return pairs.TryGetValue(pairID, out var positions) ? positions.Count : 0;
    }
}
