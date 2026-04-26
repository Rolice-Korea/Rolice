using Rolice.System;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.DebugTools
{
    public sealed class RcDebugOverlay : MonoBehaviour
    {
        private bool _visible;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 60, 30), _visible ? "DBG ▲" : "DBG ▼"))
                _visible = !_visible;

            if (!_visible) return;

            var auth    = RcBackendServices.Auth;
            var state   = RcPlayerState.Instance;
            var network = Application.internetReachability;

            string networkStr = network switch
            {
                NetworkReachability.ReachableViaLocalAreaNetwork   => "WiFi",
                NetworkReachability.ReachableViaCarrierDataNetwork => "Mobile",
                _                                                  => "None"
            };

            string uid   = "-";
            string name  = "-";
            string email = "-";
#if !UNITY_EDITOR
            var firebaseUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (firebaseUser != null)
            {
                uid  = firebaseUser.UserId.Length > 8 ? firebaseUser.UserId[..8] + "..." : firebaseUser.UserId;
                name = string.IsNullOrEmpty(firebaseUser.DisplayName) ? "(none)" : firebaseUser.DisplayName;

                // Google 로그인 시 Email은 CurrentUser.Email이 아닌 ProviderData에 존재
                email = firebaseUser.Email;
                if (string.IsNullOrEmpty(email))
                {
                    foreach (var info in firebaseUser.ProviderData)
                    {
                        if (info.ProviderId == "google.com" && !string.IsNullOrEmpty(info.Email))
                        {
                            email = info.Email;
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(email)) email = "(none)";
            }
#endif

            var lines = new[]
            {
                $"Network : {networkStr}",
                $"Auth    : {(auth.IsAuthenticated ? "OK" : "NO")}",
                $"UID     : {uid}",
                $"Name    : {name}",
                $"Email   : {email}",
                $"Synced  : {state.IsSynced}",
            };

            float w       = 260f;
            float lineH   = 22f;
            float padding = 8f;
            float totalH  = padding * 2 + lineH * lines.Length + 36f;

            GUI.Box(new Rect(0, 30, w, totalH), "");

            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(padding, 30 + padding + lineH * i, w - padding * 2, lineH), lines[i]);

            float btnY = 30 + padding + lineH * lines.Length + 4f;
            if (GUI.Button(new Rect(padding, btnY, w - padding * 2, 28f), "Sign Out"))
                auth.SignOut();
        }
    }
}
