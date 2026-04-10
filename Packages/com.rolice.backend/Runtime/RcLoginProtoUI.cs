using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// 프로토타입용 로그인 UI. 씬에 빈 오브젝트로 올려서 사용.
    /// 정식 버전에서는 RcUIPanel 기반으로 교체.
    /// </summary>
    public sealed class RcLoginProtoUI : MonoBehaviour
    {
        // 로그인 성공 시 게임 쪽에서 후처리 (클라우드 동기화 등) 등록
        public Func<UniTask> OnLoginSucceeded;

        private string _statusMessage = "로그인이 필요합니다.";
        private bool   _isBusy        = false;

        private void OnGUI()
        {
            var boxRect    = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 120, 400, 240);
            var labelRect  = new Rect(boxRect.x + 20, boxRect.y + 20,  360, 60);
            var buttonRect = new Rect(boxRect.x + 80, boxRect.y + 100, 240, 60);
            var statusRect = new Rect(boxRect.x + 20, boxRect.y + 170, 360, 40);

            GUI.Box(boxRect, "");
            GUI.Label(labelRect, "Rolice 계정 로그인");

            GUI.enabled = !_isBusy;
            if (GUI.Button(buttonRect, _isBusy ? "로그인 중..." : "Google로 로그인"))
                OnLoginButtonClicked().Forget();
            GUI.enabled = true;

            GUI.Label(statusRect, _statusMessage);
        }

        private async UniTaskVoid OnLoginButtonClicked()
        {
            _isBusy        = true;
            _statusMessage = "로그인 중...";

            try
            {
                var authService = RcBackendServices.Auth as RcFirebaseAuthService;
                if (authService == null)
                {
                    _statusMessage = "Firebase Auth가 등록되지 않았습니다.";
                    _isBusy = false;
                    return;
                }

                await authService.SignInAsync();
                _statusMessage = "로그인 성공!";

                if (OnLoginSucceeded != null)
                    await OnLoginSucceeded.Invoke();

                await UniTask.Delay(1000);
                Destroy(gameObject);
            }
            catch (global::System.Exception e)
            {
                _statusMessage = $"로그인 실패: {e.Message}";
                Debug.LogWarning($"[LoginUI] {e}");
            }
            finally
            {
                _isBusy = false;
            }
        }
    }
}
