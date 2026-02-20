using Rolice.Data;
using UnityEngine;

public static class RcAppBootstrap
{
    private const string CorePrefabPath = "Core";
    private const string StageDatabasePath = "Data/StageDatabase";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadCorePrefab();
        InitializeProgressManager();
    }

    private static void LoadCorePrefab()
    {
        var prefab = Resources.Load<GameObject>(CorePrefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[AppBootstrap] Core 프리팹을 찾을 수 없습니다: Resources/{CorePrefabPath}");
            return;
        }

        var instance = Object.Instantiate(prefab);
        instance.name = "Core";
        Object.DontDestroyOnLoad(instance);
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
