using System;
using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    /// <summary>
    /// 광고 보상 수령 이력 저장소.
    /// 서버 타임스탬프 기준으로 기록하므로 클라이언트 시간 조작 방어.
    /// </summary>
    public interface IAdRewardStorage
    {
        /// <summary>보상 수령을 서버 타임스탬프로 기록한다.</summary>
        UniTask RecordRewardAsync();

        /// <summary>마지막 수령 시각을 UTC 기준으로 반환. 이력 없으면 null.</summary>
        UniTask<DateTime?> GetLastRewardTimeAsync();
    }
}
