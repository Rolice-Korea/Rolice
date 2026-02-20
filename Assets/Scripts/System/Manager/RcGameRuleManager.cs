using Engine;
using UnityEngine;

public class RcGameRuleManager : RcSingletonMono<RcGameRuleManager>
{
    private RcLevelRules currentRules;

    private int currentTurn;
    private bool isGameOver;
    private bool isInitialized;

    public bool IsGameOver => isGameOver;
    public int CurrentTurn => currentTurn;
    public bool IsInitialized => isInitialized;

    public void Initialize(RcLevelRules rules)
    {
        if (rules == null)
        {
            Debug.LogError("[GameRuleManager] RcLevelRules가 null입니다!");
            return;
        }

        if (!rules.Validate())
        {
            Debug.LogError("[GameRuleManager] 잘못된 규칙입니다!");
            return;
        }

        UnsubscribeEvents();

        currentRules = rules;
        currentTurn = 0;
        isGameOver = false;
        isInitialized = true;

        SubscribeEvents();
    }

    public void Reset()
    {
        currentTurn = 0;
        isGameOver = false;
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
        RcGameEvents.Instance.Publish(RcGameEvent.TurnChanged, currentTurn);
        CheckLoseConditions();
    }

    public void CheckWinCondition()
    {
        if (!isInitialized || isGameOver)
            return;

        if (RcLevelManager.Instance.CheckLevelComplete())
            HandleGameWin();
    }

    private void CheckLoseConditions()
    {
        if (isGameOver)
            return;

        if (currentRules.HasTurnLimit && currentTurn >= currentRules.MaxTurns)
            HandleGameLose();
    }

    private void HandleGameWin()
    {
        isGameOver = true;
        RcGameEvents.Instance.Publish(RcGameEvent.GameWin);
    }

    private void HandleGameLose()
    {
        isGameOver = true;
        RcGameEvents.Instance.Publish(RcGameEvent.GameLose);
    }

    private void SubscribeEvents()
    {
        RcGameEvents.Instance.Subscribe(RcGameEvent.MoveStarted, OnMoveStarted);
        RcGameEvents.Instance.Subscribe(RcGameEvent.MoveCompleted, OnMoveCompleted);
        RcGameEvents.Instance.Subscribe(RcGameEvent.LevelCompleted, OnLevelCompleted);
    }

    private void UnsubscribeEvents()
    {
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.MoveStarted, OnMoveStarted);
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.MoveCompleted, OnMoveCompleted);
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.LevelCompleted, OnLevelCompleted);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void OnMoveStarted(Vector2Int pos)
    {
        if (!isInitialized || isGameOver) return;
        IncrementTurn();
    }

    private void OnMoveCompleted(Vector2Int pos)
    {
        if (!isInitialized || isGameOver) return;
        CheckWinCondition();
    }

    private void OnLevelCompleted()
    {
        CheckWinCondition();
    }

    public int GetRemainingTurns()
    {
        if (!currentRules.HasTurnLimit) return -1;
        return Mathf.Max(0, currentRules.MaxTurns - currentTurn);
    }

    public float GetTurnProgress()
    {
        if (!currentRules.HasTurnLimit) return 0f;
        return (float)currentTurn / currentRules.MaxTurns;
    }
}
