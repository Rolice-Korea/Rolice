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
    private string                   _errorMessage;
    private UniTaskCompletionSource  _retrytcs;

    private void Start()
    {
#if UNITY_EDITOR
        RcSceneLoader.Instance.LoadScene("LobbyScene");
#else
        RunAsync().Forget();
#endif
    }

    private void OnGUI()
    {
        if (_errorMessage == null) return;

        var boxRect    = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 70, 400, 140);
        var labelRect  = new Rect(boxRect.x + 20, boxRect.y + 20,  360, 60);
        var buttonRect = new Rect(boxRect.x + 80, boxRect.y + 90, 240, 40);

        GUI.Box(boxRect, "");
        GUI.Label(labelRect, _errorMessage);
        if (GUI.Button(buttonRect, "재시도"))
            _retrytcs?.TrySetResult();
    }

    private async UniTaskVoid RunAsync()
    {
        await AuthenticateAsync();
        await RcPlayerState.Instance.SyncFromCloudAsync();
        RcSceneLoader.Instance.LoadScene("LobbyScene");
    }

    // 인증 성공할 때까지 루프
    private async UniTask AuthenticateAsync()
    {
        while (true)
        {
            try
            {
                await RcBackendServices.Auth.EnsureAuthAsync();
                _errorMessage = null;
                return;
            }
            catch (AuthRequiredException)
            {
                _errorMessage = null;
                await ShowLoginAndWaitAsync();
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InitBootstrap] 인증 오류: {e.Message}");
                await ShowRetryAndWaitAsync();
                // 루프 재시도
            }
        }
    }

    private UniTask ShowLoginAndWaitAsync()
    {
        var tcs = new UniTaskCompletionSource();

        var go = new GameObject("LoginUI");
        var ui = go.AddComponent<RcLoginProtoUI>();
        ui.OnLoginSucceeded = () =>
        {
            tcs.TrySetResult();
            return UniTask.CompletedTask;
        };

        return tcs.Task;
    }

    private UniTask ShowRetryAndWaitAsync()
    {
        _errorMessage = "서버에 연결할 수 없습니다.\n네트워크 상태를 확인해 주세요.";
        _retrytcs     = new UniTaskCompletionSource();
        return _retrytcs.Task;
    }
}
