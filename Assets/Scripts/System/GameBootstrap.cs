using UnityEngine;

/// <summary>
/// 게임 초기화 부트스트랩
/// - 매니저 초기화 순서 관리
/// - 레벨 로드
/// - 이벤트 연결
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Data Assets")]
    [SerializeField] private RcMaterialDataBaseSO materialDatabase;
    
    [Header("Level Settings")]
    [SerializeField] private RcLevelDataSO startingLevel;
    [SerializeField] private Transform tilesParent;
    
    private void Awake()
    {
        Debug.Log("=== Game Bootstrap 시작 ===");
        
        // 1. 데이터 매니저 초기화
        InitializeDataManager();
        
        // 2. 시작 레벨 로드
        if (startingLevel != null)
        {
            LoadStartingLevel();
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
    
    /// <summary>
    /// 데이터 매니저 초기화
    /// </summary>
    private void InitializeDataManager()
    {
        if (materialDatabase == null)
        {
            Debug.LogError("[Bootstrap] MaterialDatabase가 할당되지 않았습니다!");
            return;
        }
        
        RcDataManager.Instance.Initialize(materialDatabase);
    }
    
    /// <summary>
    /// 시작 레벨 로드 및 게임 룰 초기화
    /// </summary>
    private void LoadStartingLevel()
    {
        // 레벨 로드
        LevelLoadResult result = RcLevelManager.Instance.LoadLevel(startingLevel, tilesParent);
        
        if (!result.Success)
        {
            Debug.LogError($"[Bootstrap] 레벨 로드 실패: {result.ErrorMessage}");
            return;
        }
        
        Debug.Log($"[Bootstrap] ✓ 레벨 로드 완료: {startingLevel.name}");
        
        // 게임 룰 매니저 초기화
        InitializeGameRules();
    }
    
    /// <summary>
    /// 게임 룰 매니저 초기화
    /// </summary>
    private void InitializeGameRules()
    {
        if (startingLevel == null || startingLevel.Rules == null)
        {
            Debug.LogWarning("[Bootstrap] LevelRules가 없습니다!");
            return;
        }
        
        RcGameRuleManager.Instance.Initialize(startingLevel.Rules);
    }
    
    /// <summary>
    /// 매니저 간 이벤트 연결
    /// </summary>
    private void SetupEventConnections()
    {
        // 레벨 완료 → 게임 승리
        RcLevelManager.Instance.OnLevelCompleted += OnLevelCompleted;
        
        // 게임 승리/패배 이벤트 구독 (필요시 UI 연결)
        RcGameRuleManager.Instance.OnGameWin += OnGameWin;
        RcGameRuleManager.Instance.OnGameLose += OnGameLose;
        RcGameRuleManager.Instance.OnTurnChanged += OnTurnChanged;
        
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