using Cysharp.Threading.Tasks;
using Rolice.System;
using Rolice.System.Backend;
using UnityEngine;

/// <summary>
/// Init씬 진입점. 인증 완료 후 로비씬으로 전환.
/// AppBootstrap(인프라 초기화)과 달리, 순차적 사용자 플로우를 담당.
/// </summary>
public sealed class RcInitBootstrap : MonoBehaviour
{
    private void Start()
    {
        RunAsync().Forget();
    }

    private async UniTaskVoid RunAsync()
    {
        await AuthenticateAsync();
        await SyncDataAsync();
        RcSceneLoader.Instance.LoadScene("LobbyScene");
    }

    private UniTask AuthenticateAsync() =>
        RcSystemDialogManager.Instance.ShowUntilSuccessAsync(async () =>
        {
            try
            {
                await RcBackendServices.Auth.EnsureAuthAsync();
            }
            catch (AuthRequiredException)
            {
                await RcBackendServices.Auth.SignInAsync();
            }
        }, "Connection failed.\nPlease check your network.");

    private UniTask SyncDataAsync() =>
        RcSystemDialogManager.Instance.ShowUntilSuccessAsync(
            () => RcPlayerState.Instance.SyncFromCloudAsync(),
            "Connection failed.\nPlease check your network.");
}
