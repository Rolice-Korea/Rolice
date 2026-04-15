using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Rolice.System.Backend
{
    /// <summary>
    /// 프로토타입용 더미 광고 서비스. 실제 SDK 없이 즉시 성공 처리.
    /// 실제 SDK(AdMob / Unity Ads) 연동 시 이 클래스 대신 교체.
    /// </summary>
    public sealed class RcDummyAdsService : IAdsService
    {
        public bool IsReady => true;

        public async UniTask<bool> ShowRewardedAdAsync()
        {
            Debug.Log("[AdsService] Dummy 보상형 광고 시청 완료");
            await UniTask.Yield();
            return true;
        }
    }
}
