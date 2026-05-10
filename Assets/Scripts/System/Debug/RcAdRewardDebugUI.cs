using Cysharp.Threading.Tasks;
using Rolice.Define;
using Rolice.System;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.DebugTools
{
    /// <summary>
    /// 광고 보상 기능 임시 검증용 OnGUI. 에디터 전용.
    /// 로비씬 아무 오브젝트에나 붙여서 사용.
    /// </summary>
    public sealed class RcAdRewardDebugUI : MonoBehaviour
    {
        private string _statusMessage = "";
        private bool   _isBusy        = false;

        private void OnGUI()
        {
            var panelRect  = new Rect(20, 20, 260, 140);
            var gemRect    = new Rect(30, 30, 240, 25);
            var buttonRect = new Rect(30, 60, 240, 30);
            var resetRect  = new Rect(30, 95, 240, 25);
            var statusRect = new Rect(30, 122, 240, 25);

            GUI.Box(panelRect, "");

            int gems = RcBackendServices.Economy.GetBalance(RcCurrencyId.Gem.ToKey());
            GUI.Label(gemRect, $"💎 Gem: {gems}");

            GUI.enabled = !_isBusy;
            if (GUI.Button(buttonRect, _isBusy ? "처리 중..." : "광고 보고 보석 받기"))
                ClaimAsync().Forget();
            if (GUI.Button(resetRect, "수령 이력 초기화 (테스트용)"))
                ResetAsync().Forget();
            GUI.enabled = true;

            GUI.Label(statusRect, _statusMessage);
        }

        private async UniTaskVoid ResetAsync()
        {
            _isBusy = true;
            await RcBackendServices.AdReward.ClearAsync();
            _statusMessage = "이력 초기화 완료";
            _isBusy = false;
        }

        private async UniTaskVoid ClaimAsync()
        {
            _isBusy        = true;
            _statusMessage = "";

            var result = await RcAdRewardService.Instance.ClaimAsync();

            _statusMessage = result switch
            {
                AdRewardResult.Success            => "✓ 보석 +1 지급 완료",
                AdRewardResult.AlreadyClaimedToday => "오늘 이미 수령했습니다",
                AdRewardResult.NotLoggedIn         => "로그인이 필요합니다",
                AdRewardResult.AdFailed            => "광고 시청 실패",
                _                                  => "알 수 없는 오류"
            };

            _isBusy = false;
        }
    }
}
