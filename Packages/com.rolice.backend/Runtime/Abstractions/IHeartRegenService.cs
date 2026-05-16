using System;
using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    /// <summary>
    /// 하트 시간 회복 서비스.
    /// 회복 기점 타임스탬프는 Firestore 서버 시각 기준 — 클라이언트 시간 조작 방어.
    /// 잔액 읽기는 IPlayerDataCache(로컬), 쓰기는 Firestore 트랜잭션(서버 권위).
    /// </summary>
    public interface IHeartRegenService
    {
        /// <summary>
        /// 앱 시작 시 호출. 경과 시간을 계산해 회복된 하트를 Firestore에 반영하고 로컬 캐시를 갱신한다.
        /// 신규 유저는 maxHearts로 초기화. 잔액 >= maxHearts 시 회복 스킵 (초과 구매분 보호).
        /// </summary>
        UniTask SyncRegenAsync(int maxHearts);

        /// <summary>
        /// 하트 1개 소모. 잔액 부족 시 false. 네트워크/서버 오류 시 예외.
        /// 소모 전 잔액이 maxHearts였다면 타이머 시작(서버 타임스탬프 갱신).
        /// </summary>
        UniTask<bool> SpendHeartAsync(int maxHearts);

        /// <summary>마지막 회복 기점(UTC). 타이머 계산용. 기록 없으면 null.</summary>
        UniTask<DateTime?> GetLastRegenAtAsync();
    }
}
