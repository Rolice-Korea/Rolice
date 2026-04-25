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
        }, "서버에 연결할 수 없습니다.\n네트워크 상태를 확인해 주세요.");

    private UniTask SyncDataAsync() =>
        RcSystemDialogManager.Instance.ShowUntilSuccessAsync(
            () => RcPlayerState.Instance.SyncFromCloudAsync(),
            "서버에 연결할 수 없습니다.\n네트워크 상태를 확인해 주세요.");
}
