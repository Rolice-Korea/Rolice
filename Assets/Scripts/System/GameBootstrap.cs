using UnityEngine;

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
    
    private void LoadStartingLevel()
    {
        RcLevelManager.Instance.LoadLevel(startingLevel, tilesParent);
        Debug.Log($"[Bootstrap] 시작 레벨 로드: {startingLevel.name}");
    }
}