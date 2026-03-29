using Engine;
using UnityEngine;

public class RcGameRuleManager : RcSingletonMono<RcGameRuleManager>
{
    private RcLevelRules currentRules;

    private int currentTurn;
    private float elapsedTime;
    private bool isTimerRunning;
    private bool isGameOver;
    private bool isInitialized;

    public bool IsGameOver    => isGameOver;
    public int  CurrentTurn   => currentTurn;
    public float ElapsedTime  => elapsedTime;
    public bool IsInitialized => isInitialized;

    public void Initialize(RcLevelRules rules)
    {
        if (rules == null)
        {
            Debug.LogError("[GameRuleManager] RcLevelRules가 null입니다!");
            return;
        }

        UnsubscribeEvents();

        currentRules    = rules;
        currentTurn     = 0;
        elapsedTime     = 0f;
        isTimerRunning  = false;
        isGameOver      = false;
        isInitialized   = true;

        SubscribeEvents();
    }

    public void Reset()
    {
        currentTurn    = 0;
        elapsedTime    = 0f;
        isTimerRunning = false;
        isGameOver     = false;
    }

    private void Update()
    {
        if (isTimerRunning)
            elapsedTime += Time.deltaTime;
    }

    public void IncrementTurn()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[GameRuleManager] 초기화되지 않았습니다!");
            return;
        }

        if (isGameOver) return;

        currentTurn++;
        RcGameEvents.Instance.Publish(RcGameEvent.TurnChanged, currentTurn);
    }

    public void CheckWinCondition()
    {
        if (!isInitialized || isGameOver) return;

        if (RcLevelManager.Instance.CheckLevelComplete())
            HandleGameWin();
    }

    private void HandleGameWin()
    {
        isGameOver     = true;
        isTimerRunning = false;
        RcGameEvents.Instance.Publish(RcGameEvent.GameWin);
    }

    private void SubscribeEvents()
    {
        RcGameEvents.Instance.Subscribe(RcGameEvent.MoveStarted,   OnMoveStarted);
        RcGameEvents.Instance.Subscribe(RcGameEvent.MoveCompleted, OnMoveCompleted);
        RcGameEvents.Instance.Subscribe(RcGameEvent.LevelCompleted, OnLevelCompleted);
    }

    private void UnsubscribeEvents()
    {
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.MoveStarted,   OnMoveStarted);
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

        // 첫 이동 시 타이머 시작
        if (!isTimerRunning)
            isTimerRunning = true;

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
}
