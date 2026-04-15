using System;
using Cysharp.Threading.Tasks;
using Engine;
using Rolice.Define;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.System
{
    public enum AdRewardResult
    {
        Success,
        NotLoggedIn,
        AlreadyClaimedToday,
        AdFailed,
    }

    /// <summary>
    /// 광고 보상 비즈니스 로직 싱글톤.
    /// 로그인 확인 → 하루 1회 제한(서버 시간 기준) → 광고 시청 → Gem 지급 → 이력 기록 순서로 처리.
    /// UI는 이 서비스를 통해 트리거하고, 결과에 따라 화면을 구성한다.
    /// </summary>
    public sealed class RcAdRewardService : RcSingleton<RcAdRewardService>
    {
        /// <summary>
        /// 오늘 보상을 수령할 수 있는지 확인한다.
        /// 비로그인이면 false. 이미 오늘 수령했으면 false.
        /// </summary>
        public async UniTask<bool> CanClaimAsync()
        {
            if (!RcBackendServices.Auth.IsAuthenticated) return false;

            DateTime? lastReward = await RcBackendServices.AdReward.GetLastRewardTimeAsync();
            if (lastReward == null) return true;

            // UTC 기준 날짜 비교 (서버 타임스탬프에서 읽어온 DateTime은 UTC)
            return lastReward.Value.Date < DateTime.UtcNow.Date;
        }

        /// <summary>
        /// 광고를 시청하고 보상(Gem 1개)을 지급한다.
        /// </summary>
        public async UniTask<AdRewardResult> ClaimAsync()
        {
            if (!RcBackendServices.Auth.IsAuthenticated)
            {
                Debug.Log("[AdReward] 보상 불가: 비로그인 상태");
                return AdRewardResult.NotLoggedIn;
            }

            DateTime? lastReward = await RcBackendServices.AdReward.GetLastRewardTimeAsync();
            if (lastReward != null && lastReward.Value.Date >= DateTime.UtcNow.Date)
            {
                Debug.Log("[AdReward] 보상 불가: 오늘 이미 수령함");
                return AdRewardResult.AlreadyClaimedToday;
            }

            bool watched = await RcBackendServices.Ads.ShowRewardedAdAsync();
            if (!watched)
            {
                Debug.Log("[AdReward] 보상 불가: 광고 시청 실패 또는 취소");
                return AdRewardResult.AdFailed;
            }

            RcBackendServices.Economy.Add(RcCurrencyId.Gem.ToKey(), 1);
            await RcBackendServices.AdReward.RecordRewardAsync();

            Debug.Log("[AdReward] 보상 지급 완료: Gem +1");
            return AdRewardResult.Success;
        }
    }
}
