using Engine.UI;
using Rolice.UI;
using UnityEngine;

public class RcGameBootstrap : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private Transform tilesParent;

    [Header("Fallback (에디터 직접 실행용)")]
    [SerializeField] private RcLevelDataSO fallbackLevel;

    private int currentStageNumber;

    private void Awake()
    {
        RcLevelDataSO levelData = ResolveLevel();
        if (levelData != null)
        {
            GameLoad(levelData);
        }
        else
        {
            Debug.LogError("[Bootstrap] 로드할 레벨을 찾을 수 없습니다!");
        }
    }

    private RcLevelDataSO ResolveLevel()
    {
        int stageNumber = RcGameContext.SelectedStageNumber;

        if (stageNumber > 0)
        {
            var levelData = RcProgressManager.Instance.StageDatabase.GetStage(stageNumber);
            if (levelData != null)
            {
                currentStageNumber = stageNumber;
                return levelData;
            }

            Debug.LogWarning($"[Bootstrap] 스테이지 {stageNumber} 데이터를 찾을 수 없습니다. fallback 사용");
        }

        if (fallbackLevel == null) return null;

        currentStageNumber = fallbackLevel.StageInfo.StageNumber;
        return fallbackLevel;
    }

    private void GameLoad(RcLevelDataSO levelData)
    {
        LoadLevel(levelData);
    }

    private void Start()
    {
        RcUIManager.Instance.Open<RcGameHudPanel>();
        RcGameResultManager.Instance.Initialize(currentStageNumber);
    }

    private void LoadLevel(RcLevelDataSO levelData)
    {
        RcLevelLoadResult result = RcLevelManager.Instance.LoadLevel(levelData, tilesParent);

        if (!result.Success)
        {
            Debug.LogError($"[Bootstrap] 레벨 로드 실패: {result.ErrorMessage}");
            return;
        }

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
}
