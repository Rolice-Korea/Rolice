using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public interface IAdsService
    {
        /// <summary>광고 SDK가 준비 상태인지.</summary>
        bool IsReady { get; }

        /// <summary>
        /// 보상형 광고를 표시한다.
        /// 완료(보상 지급 가능) 시 true, 취소·실패 시 false 반환.
        /// </summary>
        UniTask<bool> ShowRewardedAdAsync();
    }
}
