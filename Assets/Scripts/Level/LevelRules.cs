using System;
using UnityEngine;

/// 레벨별 게임 규칙 데이터
/// - 턴 제한, 시간 제한 등의 조건 설정
/// - LevelDataSO에 포함되어 Inspector에서 설정
[Serializable]
public class LevelRules
{
    [Header("Turn Limit")]
    [Tooltip("턴 제한을 사용할지 여부")]
    public bool HasTurnLimit = true;
    
    [Tooltip("최대 턴 수")]
    public int MaxTurns = 10;
    
    [Header("Time Limit")]
    [Tooltip("시간 제한을 사용할지 여부")]
    public bool HasTimeLimit = false;
    
    [Tooltip("제한 시간 (초)")]
    public float MaxTime = 60f;
    
    // === 나중에 추가할 수 있는 조건들 ===
    // public bool RequireSpecialOrder = false;
    // public Vector2Int[] SpecialTileOrder;
    
    // public bool HasScoreRequirement = false;
    // public int MinScore = 1000;
    
    public bool Validate()
    {
        if (HasTurnLimit && MaxTurns <= 0)
        {
            Debug.LogWarning("[LevelRules] MaxTurns는 0보다 커야 합니다");
            return false;
        }
        
        if (HasTimeLimit && MaxTime <= 0f)
        {
            Debug.LogWarning("[LevelRules] MaxTime은 0보다 커야 합니다");
            return false;
        }
        
        return true;
    }
}
