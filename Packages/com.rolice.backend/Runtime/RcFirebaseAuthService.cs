using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Google;
using UnityEngine;

namespace Rolice.System.Backend
{
    public sealed class RcFirebaseAuthService : IAuthService
    {
        private const string WebClientId = "972066935813-6e3cv2l05o0ijp922qi17r8l8dkucpo9.apps.googleusercontent.com";

        private FirebaseAuth _auth;

        public bool IsAuthenticated => _auth?.CurrentUser != null;

        public RcFirebaseAuthService()
        {
            _auth = FirebaseAuth.DefaultInstance;
        }

        public async UniTask EnsureAuthAsync()
        {
#if UNITY_EDITOR
            // 에디터: Google 계정 캐시가 남아있을 수 있으므로 익명 계정이 아니면 로그아웃 후 재로그인
            if (_auth.CurrentUser != null && !_auth.CurrentUser.IsAnonymous)
                _auth.SignOut();

            if (!IsAuthenticated)
                await _auth.SignInAnonymouslyAsync();

            Debug.Log($"[FirebaseAuth] 에디터 익명 로그인 완료 | UID: {_auth.CurrentUser?.UserId} | IsAnonymous: {_auth.CurrentUser?.IsAnonymous}");
#else
            if (IsAuthenticated) return;

            // 1. 자동(조용한) 로그인 먼저 시도
            try
            {
                await SignInSilentlyAsync();
                return;
            }
            catch (Exception e)
            {
                Debug.Log($"[FirebaseAuth] 자동 로그인 실패, 수동 로그인 필요: {e.Message}");
                throw new AuthRequiredException();
            }
#endif
        }

        // 자동 로그인 (계정 선택 UI 없음)
        public async UniTask SignInSilentlyAsync()
        {
            ConfigureGoogleSignIn();
            var result = await GoogleSignIn.DefaultInstance.SignInSilently();
            await FirebaseSignInWithGoogleAsync(result);
        }

        // 수동 로그인 (계정 선택 UI 표시)
        public async UniTask SignInAsync()
        {
            ConfigureGoogleSignIn();
            var result = await GoogleSignIn.DefaultInstance.SignIn();
            await FirebaseSignInWithGoogleAsync(result);
        }

        public void SignOut()
        {
            _auth.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
        }

        private void ConfigureGoogleSignIn()
        {
            if (GoogleSignIn.Configuration != null) return;

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId    = WebClientId,
                RequestIdToken = true,
                UseGameSignIn  = false
            };
        }

        private async UniTask FirebaseSignInWithGoogleAsync(GoogleSignInUser user)
        {
            var credential = GoogleAuthProvider.GetCredential(user.IdToken, null);
            await _auth.SignInWithCredentialAsync(credential);
            Debug.Log($"[FirebaseAuth] 로그인 성공: {_auth.CurrentUser.DisplayName}");
        }
    }

    // 수동 로그인이 필요한 상황을 알리는 예외
    public sealed class AuthRequiredException : Exception
    {
        public AuthRequiredException() : base("Google 로그인이 필요합니다.") { }
    }
}
