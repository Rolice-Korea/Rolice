using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Rolice.System.Backend
{
    public sealed class RcAdMobAdsService : IAdsService
    {
        // TODO [릴리즈 전] 아래 ID를 실제 광고 단위 ID로 교체: ca-app-pub-9571081050294621/8375468011
        private const string AdUnitId = "ca-app-pub-3940256099942544/5224354917";

        private RewardedAd _ad;

        public bool IsReady => _ad != null;

        public RcAdMobAdsService()
        {
            MobileAds.Initialize(_ => LoadAd());
        }

        private void LoadAd()
        {
            _ad?.Destroy();
            _ad = null;

            RewardedAd.Load(AdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning($"[AdsService] 로드 실패: {error}");
                    return;
                }
                _ad = ad;
            });
        }

        public async UniTask<bool> ShowRewardedAdAsync()
        {
            if (!IsReady)
            {
                Debug.LogWarning("[AdsService] 광고 준비 안됨");
                return false;
            }

            var tcs = new UniTaskCompletionSource<bool>();
            bool rewarded = false;

            _ad.OnAdFullScreenContentClosed += () =>
            {
                tcs.TrySetResult(rewarded);
                LoadAd();
            };

            _ad.OnAdFullScreenContentFailed += (AdError err) =>
            {
                Debug.LogWarning($"[AdsService] 표시 실패: {err}");
                tcs.TrySetResult(false);
                LoadAd();
            };

            _ad.Show(reward => rewarded = true);

            return await tcs.Task;
        }
    }
}
