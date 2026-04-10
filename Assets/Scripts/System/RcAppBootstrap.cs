using Cysharp.Threading.Tasks;
using Rolice.Data;
using Rolice.System;
using Rolice.System.Backend;
using UnityEngine;

public static class RcAppBootstrap
{
    private const string CorePrefabPath    = "Core";
    private const string StageDatabasePath = "StageDatabase";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadCorePrefab();
        RcBackendServices.Register(new RcFirebaseProvider());
        RcPlayerState.Instance.Initialize();
        EnsureLoginAsync().Forget();
        InitializeProgressManager();
        RcScreenOrientationApplier.Apply(RcGameSettingsData.Current.GetScreenMode());
    }

    private static async UniTaskVoid EnsureLoginAsync()
    {
        try
        {
            await RcBackendServices.Auth.EnsureAuthAsync();
        }
        catch (AuthRequiredException)
        {
            // 자동 로그인 실패 — 로그인 UI 표시 (중복 방지)
            if (Object.FindObjectOfType<RcLoginProtoUI>() != null) return;
            var go = new GameObject("LoginUI");
            var ui = go.AddComponent<RcLoginProtoUI>();
            ui.OnLoginSucceeded = () => RcPlayerState.Instance.SyncFromCloudAsync();
            Object.DontDestroyOnLoad(go);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AppBootstrap] 인증 실패: {e.Message}");
        }
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
