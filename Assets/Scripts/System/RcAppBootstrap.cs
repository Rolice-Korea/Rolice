using Rolice.Data;
using Rolice.System;
using Rolice.System.Backend;
using UnityEngine;

/// <summary>
/// 앱 전역 인프라 초기화. 씬 독립적인 싱글톤·서비스 등록만 담당.
/// 인증/데이터 싱크 등 사용자 플로우는 RcInitBootstrap(Init씬)이 처리.
/// </summary>
public static class RcAppBootstrap
{
    private const string CorePrefabPath = "Core";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadCorePrefab();
        RegisterBackend();
        RcScreenOrientationApplier.Apply(RcGameSettingsData.Current.GetScreenMode());
    }

    private static void RegisterBackend()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var devSettings = Resources.Load<RcDeveloperSettingsSO>("DeveloperSettings");
        if (devSettings != null && devSettings.offlineMode)
        {
            RcBackendServices.RegisterOffline(devSettings.backendAlwaysSucceed);
            return;
        }
#endif
        RcBackendServices.Register(new RcDefaultBackend(RcPlayerState.Instance));
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
}
