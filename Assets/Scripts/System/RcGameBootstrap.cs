using System;
using UnityEngine;


public class RcGameBootstrap : MonoBehaviour
{
    [Header("Data Assets")]
    [SerializeField] private RcMaterialDataBaseSO materialDatabase;
    
    [Header("Level Settings")]
    [SerializeField] private RcLevelDataSO startingLevel;
    [SerializeField] private Transform tilesParent;
    
    [Header("Temp")]
    [SerializeField] private RcLevelDataSO tempLevelData;
    
    private void Awake()
    {
        GameLoad(startingLevel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameLoad(tempLevelData);
        }
    }

    private void GameLoad(RcLevelDataSO levelData)
    {
        UnsubscribeEventConnections();
        
        Debug.Log("=== Game Bootstrap 시작 ===");
        
        // 1. 데이터 매니저 초기화
        InitializeDataManager();
        
        // 2. 시작 레벨 로드
        if (levelData != null)
        {
            LoadLevel(levelData);
        }
        else
        {
            Debug.LogWarning("[Bootstrap] 시작 레벨이 설정되지 않았습니다!");
        }
        
        // 3. 이벤트 연결
        SetupEventConnections();
        
        Debug.Log("=== Game Bootstrap 완료 ===");
    }
    
    private void OnDestroy()
    {
        // 이벤트 연결 해제
        if (RcLevelManager.Instance != null)
        {
            RcLevelManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }
    }
    

    private void InitializeDataManager()
    {
        if (materialDatabase == null)
        {
            Debug.LogError("[Bootstrap] MaterialDatabase가 할당되지 않았습니다!");
            return;
        }
        
        RcDataManager.Instance.Initialize(materialDatabase);
    }
    
    private void LoadLevel(RcLevelDataSO levelData)
    {
        // 레벨 로드
        RcLevelLoadResult result = RcLevelManager.Instance.LoadLevel(levelData, tilesParent);
        
        if (!result.Success)
        {
            Debug.LogError($"[Bootstrap] 레벨 로드 실패: {result.ErrorMessage}");
            return;
        }
        
        Debug.Log($"[Bootstrap] ✓ 레벨 로드 완료: {startingLevel.name}");
        
        // 게임 룰 매니저 초기화
        InitializeGameRules(levelData);
    }
    

    private void InitializeGameRules(RcLevelDataSO levelData)
    {
        if (levelData == null || levelData.Rules == null)
        {
            Debug.LogWarning("[Bootstrap] LevelRules가 없습니다!");
            return;
        }
        
        RcGameRuleManager.Instance.Initialize(levelData.Rules);
    }
    
    private void SetupEventConnections()
    {
        // 레벨 완료 → 게임 승리
        RcLevelManager.Instance.OnLevelCompleted += OnLevelCompleted;
        
        // 게임 승리/패배 이벤트 구독 (필요시 UI 연결)
        RcGameRuleManager.Instance.OnGameWin += OnGameWin;
        RcGameRuleManager.Instance.OnGameLose += OnGameLose;
        RcGameRuleManager.Instance.OnTurnChanged += OnTurnChanged;
    }

    private void UnsubscribeEventConnections()
    {
        RcLevelManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        RcGameRuleManager.Instance.OnGameWin -= OnGameWin;
        RcGameRuleManager.Instance.OnGameLose -= OnGameLose;
        RcGameRuleManager.Instance.OnTurnChanged -= OnTurnChanged;
    }
    
    // === 이벤트 핸들러 ===
    
    private void OnLevelCompleted()
    {
        RcGameRuleManager.Instance.CheckWinCondition();
    }
    
    private void OnGameWin()
    {
        Debug.Log("[Bootstrap] 승리 처리");
        // TODO: 승리 UI 표시, 다음 레벨 로드 등
    }
    
    private void OnGameLose()
    {
        Debug.Log("[Bootstrap] 패배 처리");
        // TODO: 패배 UI 표시, 재시작 옵션 등
    }
    
    private void OnTurnChanged(int currentTurn)
    {
        // TODO: UI 업데이트
        // Debug.Log($"[Bootstrap] 턴 UI 업데이트: {currentTurn}");
    }
}