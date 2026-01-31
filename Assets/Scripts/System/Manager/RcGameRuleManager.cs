using System;
using Engine;
using UnityEngine;

public class RcGameRuleManager : RcSingletonMono<RcGameRuleManager>
{
    
    private LevelRules currentRules;
    
    private int currentTurn;
    private float elapsedTime;
    private bool isGameOver;
    private bool isInitialized;
    
    public event Action OnGameWin;
    public event Action OnGameLose;
    public event Action<int> OnTurnChanged;  // UI 업데이트용
    
    public bool IsGameOver => isGameOver;
    public int CurrentTurn => currentTurn;
    public float ElapsedTime => elapsedTime;
    public bool IsInitialized => isInitialized;
    
    public void Initialize(LevelRules rules)
    {
        if (rules == null)
        {
            Debug.LogError("[GameRuleManager] LevelRules가 null입니다!");
            return;
        }
        
        if (!rules.Validate())
        {
            Debug.LogError("[GameRuleManager] 잘못된 규칙입니다!");
            return;
        }
        
        currentRules = rules;
        currentTurn = 0;
        elapsedTime = 0f;
        isGameOver = false;
        isInitialized = true;
        
        Debug.Log($"[GameRuleManager] 규칙 초기화 완료");
        Debug.Log($"  - 턴 제한: {(rules.HasTurnLimit ? $"{rules.MaxTurns}턴" : "없음")}");
        Debug.Log($"  - 시간 제한: {(rules.HasTimeLimit ? $"{rules.MaxTime}초" : "없음")}");
    }
    
    public void Reset()
    {
        currentTurn = 0;
        elapsedTime = 0f;
        isGameOver = false;
        
        Debug.Log("[GameRuleManager] 게임 룰 리셋");
    }
    
    void Update()
    {
        if (!isInitialized || isGameOver || currentRules == null) 
            return;
        
        // 시간 제한 추적
        if (currentRules.HasTimeLimit)
        {
            elapsedTime += Time.deltaTime;
            CheckLoseConditions();
        }
    }

    public void IncrementTurn()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[GameRuleManager] 초기화되지 않았습니다!");
            return;
        }
        
        if (isGameOver) 
            return;
        
        currentTurn++;
        OnTurnChanged?.Invoke(currentTurn);
        
        Debug.Log($"[GameRuleManager] 턴 증가: {currentTurn}/{(currentRules.HasTurnLimit ? currentRules.MaxTurns.ToString() : "∞")}");
        
        CheckLoseConditions();
    }

    public void CheckWinCondition()
    {
        if (!isInitialized || isGameOver) 
            return;
        
        if (RcLevelManager.Instance.CheckLevelComplete())
        {
            HandleGameWin();
        }
    }
    
    private void CheckLoseConditions()
    {
        if (isGameOver) 
            return;
        
        // 턴 제한 체크
        if (currentRules.HasTurnLimit && currentTurn >= currentRules.MaxTurns)
        {
            HandleGameLose("턴 제한 초과");
            return;
        }
        
        // 시간 제한 체크
        if (currentRules.HasTimeLimit && elapsedTime >= currentRules.MaxTime)
        {
            HandleGameLose("시간 초과");
            return;
        }
    }
    
    private void HandleGameWin()
    {
        isGameOver = true;

        Debug.Log("║     🎉 게임 승리! 🎉     ║");
        
        OnGameWin?.Invoke();
    }
    
    private void HandleGameLose(string reason)
    {
        isGameOver = true;

        Debug.Log("║     ❌ 게임 패배 ❌      ║");
        
        OnGameLose?.Invoke();
    }
    
    // === UI용 헬퍼 메서드 ===
    
    public int GetRemainingTurns()
    {
        if (!currentRules.HasTurnLimit) return -1;
        return Mathf.Max(0, currentRules.MaxTurns - currentTurn);
    }
    
    public float GetRemainingTime()
    {
        if (!currentRules.HasTimeLimit) return -1f;
        return Mathf.Max(0f, currentRules.MaxTime - elapsedTime);
    }
    
    public float GetTurnProgress()
    {
        if (!currentRules.HasTurnLimit) return 0f;
        return (float)currentTurn / currentRules.MaxTurns;
    }
    
    public float GetTimeProgress()
    {
        if (!currentRules.HasTimeLimit) return 0f;
        return elapsedTime / currentRules.MaxTime;
    }
}