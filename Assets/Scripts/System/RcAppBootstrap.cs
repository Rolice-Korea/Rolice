using Rolice.Data;
using UnityEngine;

/// <summary>
/// 앱 전체 초기화 (씬 로드 전 자동 실행)
/// </summary>
public static class RcAppBootstrap
{
    private const string StageDatabasePath = "Data/StageDatabase";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Debug.Log("[AppBootstrap] 앱 초기화 시작");

        InitializeProgressManager();

        Debug.Log("[AppBootstrap] 앱 초기화 완료");
    }

    private static void InitializeProgressManager()
    {
        var stageDatabase = Resources.Load<RcStageDatabaseSO>(StageDatabasePath);

        if (stageDatabase == null)
        {
            Debug.LogWarning($"[AppBootstrap] StageDatabase를 찾을 수 없습니다: Resources/{StageDatabasePath}");
            return;
        }

        RcProgressManager.Instance.Initialize(stageDatabase);
    }
}
