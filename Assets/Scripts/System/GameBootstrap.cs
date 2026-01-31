using UnityEngine;

/// <summary>
/// 게임 초기화를 담당하는 Bootstrap 클래스
/// - 데이터 매니저 초기화
/// - 레벨 로드
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
    }
    
    /// <summary>
    /// 데이터 매니저를 초기화합니다
    /// </summary>
    private void InitializeDataManager()
    {
        if (materialDatabase == null)
        {
            Debug.LogError("[Bootstrap] MaterialDatabase가 할당되지 않았습니다!");
            return;
        }
        
        RcDataManager.Instance.Initialize(materialDatabase);
        Debug.Log("[Bootstrap] 데이터 매니저 초기화 완료");
    }
    
    /// <summary>
    /// 시작 레벨을 로드합니다
    /// </summary>
    private void LoadStartingLevel()
    {
        RcLevelManager.Instance.LoadLevel(startingLevel, tilesParent);
        Debug.Log($"[Bootstrap] 시작 레벨 로드: {startingLevel.name}");
    }
}