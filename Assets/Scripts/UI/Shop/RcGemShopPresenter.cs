using System;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice.Define;
using Rolice.System;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 잼 구매 팝업 프레젠터.
    /// 광고 보상은 RcAdRewardService(하루 1회)에 위임한다. IAP는 인프라 미구현(뷰에서 비활성).
    /// </summary>
    public class RcGemShopPresenter : RcUIPresenter<RcGemShopPanel>
    {
        private bool processing;

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleClose;
            Panel.OnAdClicked    += HandleAd;

            RcPlayerState.Instance.OnProgressChanged += RefreshBalance;

            RefreshBalance();
            RefreshAdAvailability().Forget();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnAdClicked    -= HandleAd;

            RcPlayerState.Instance.OnProgressChanged -= RefreshBalance;
        }

        private void RefreshBalance()
        {
            if (Panel == null) return;
            Panel.SetGemBalance(RcBackendServices.Economy.GetBalance(RcCurrencyId.Gem));
        }

        /// <summary>광고 수령 가능 여부는 서버 시각 기준(비동기)이라 별도로 갱신한다.</summary>
        private async UniTaskVoid RefreshAdAvailability()
        {
            bool canClaim = await RcAdRewardService.Instance.CanClaimAsync();
            if (Panel == null) return;

            Panel.SetAdInteractable(!processing && canClaim);
            Panel.SetAdStatusText(canClaim ? "광고 보고 잼 +1" : "오늘 수령 완료");
        }

        private void HandleClose() => Panel.RequestClose();

        private void HandleAd() => HandleAdAsync().Forget();

        private async UniTaskVoid HandleAdAsync()
        {
            if (processing) return;

            processing = true;
            Panel.SetAdInteractable(false);
            try
            {
                var result = await RcAdRewardService.Instance.ClaimAsync();
                switch (result)
                {
                    case AdRewardResult.Success:
                        Panel.SetAdStatusText("잼 +1 지급 완료");
                        break;
                    case AdRewardResult.AlreadyClaimedToday:
                        Panel.SetAdStatusText("오늘 수령 완료");
                        break;
                    case AdRewardResult.NotLoggedIn:
                        Panel.SetAdStatusText("로그인이 필요합니다");
                        break;
                    case AdRewardResult.AdFailed:
                        Panel.SetAdStatusText("광고 시청 실패");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RcGemShop] 광고 보상 실패: {e.Message}");
                Panel.SetAdStatusText("오류가 발생했습니다");
            }
            finally
            {
                processing = false;
                RefreshBalance();
                RefreshAdAvailability().Forget();
            }
        }
    }
}
