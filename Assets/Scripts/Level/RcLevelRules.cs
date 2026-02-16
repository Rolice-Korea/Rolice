using System;
using UnityEngine;

[Serializable]
public class RcLevelRules
{
    [Header("Turn Limit")]
    [Tooltip("턴 제한을 사용할지 여부")]
    public bool HasTurnLimit = true;

    [Tooltip("최대 턴 수")]
    public int MaxTurns = 10;

    public bool Validate()
    {
        if (HasTurnLimit && MaxTurns <= 0)
        {
            Debug.LogWarning("[LevelRules] MaxTurns는 0보다 커야 합니다");
            return false;
        }

        return true;
    }
}
