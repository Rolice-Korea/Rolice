using System;
using Cysharp.Threading.Tasks;
using Engine;
using Rolice.Define;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.System
{
    /// <summary>
    /// 하트 시스템 게임 레이어 싱글톤.
    ///
    /// 책임:
    ///   - 앱 시작 시 Firestore 기반 리젠 동기화 (RcInitBootstrap에서 호출)
    ///   - 스테이지 진입 시 하트 소모 게이팅
    ///   - 타이머 계산 (UI가 폴링으로 사용)
    ///
    /// 하트 잔액 원본은 IEconomyService 로컬 캐시 → RcPlayerState.GetCurrency.
    /// 회복 타임스탬프는 Firestore 서버 시각 기준 (클라이언트 시간 조작 방어).
    /// </summary>
    public sealed class RcHeartManager : RcSingleton<RcHeartManager>
    {
        public const int MaxHearts         = 5;
        public const int RegenIntervalSec  = 600;

        private DateTime? _lastRegenAt;

        /// <summary>현재 하트 잔액 (로컬 캐시에서 즉시 반환).</summary>
        public int Count => RcBackendServices.Economy.GetBalance(RcCurrencyId.Heart.ToKey());

        public bool IsFull    => Count >= MaxHearts;
        public bool HasHearts => Count > 0;

        /// <summary>UI가 구독해 하트 변경을 감지한다.</summary>
        public event Action OnChanged;

        // ─── 앱 시작 동기화 ───────────────────────────────────────────────────

        /// <summary>
        /// RcInitBootstrap.SyncDataAsync 에서 SyncFromCloudAsync 직후 호출.
        /// 경과 시간만큼 하트 회복 후 lastRegenAt 캐시.
        /// </summary>
        public async UniTask SyncAsync()
        {
            await RcBackendServices.HeartRegen.SyncRegenAsync(MaxHearts);
            _lastRegenAt = await RcBackendServices.HeartRegen.GetLastRegenAtAsync();
            OnChanged?.Invoke();
        }

        // ─── 스테이지 진입 ────────────────────────────────────────────────────

        /// <summary>
        /// 하트 1개 소모 시도.
        /// 성공 시 true. 잔액 부족 시 false. 네트워크 오류 시 예외(호출부가 처리).
        /// </summary>
        public async UniTask<bool> TrySpendAsync()
        {
            bool wasFull = IsFull;

            bool success = await RcBackendServices.HeartRegen.SpendHeartAsync(MaxHearts);
            if (!success) return false;

            // 최대치에서 소모: 로컬 타이머 기점을 낙관적으로 지금으로 설정
            // (서버 타임스탬프와의 소폭 오차는 다음 SyncAsync에서 보정)
            if (wasFull)
                _lastRegenAt = DateTime.UtcNow;

            OnChanged?.Invoke();
            return true;
        }

        // ─── 충전 (구매) ──────────────────────────────────────────────────────

        /// <summary>
        /// 하트 충전 시도. count개를 최대치까지 채운다.
        /// 실제 추가된 개수를 반환(이미 가득이면 0). 네트워크 오류 시 예외(호출부가 처리).
        /// 잼 차감은 호출부(상점)가 선행한다 — 이 메서드는 하트 증가만 담당.
        /// </summary>
        public async UniTask<int> RefillAsync(int count = 1)
        {
            int added = await RcBackendServices.HeartRegen.AddHeartAsync(count, MaxHearts);
            if (added > 0) OnChanged?.Invoke();
            return added;
        }

        // ─── 타이머 ──────────────────────────────────────────────────────────

        /// <summary>
        /// 다음 하트가 회복될 때까지 남은 초.
        /// 최대치이거나 타임스탬프 정보 없으면 -1.
        /// UI는 매 프레임 또는 1초 간격으로 이 값을 폴링한다.
        /// </summary>
        public float GetSecondsUntilRegen()
        {
            if (IsFull || _lastRegenAt == null) return -1f;

            double elapsed   = (DateTime.UtcNow - _lastRegenAt.Value).TotalSeconds;
            double remainder = elapsed % RegenIntervalSec;
            return (float)(RegenIntervalSec - remainder);
        }

        /// <summary>현재 리젠 사이클 내 진행률 (0~1). 타이머 바 UI용.</summary>
        public float GetRegenProgress()
        {
            float remaining = GetSecondsUntilRegen();
            if (remaining < 0f) return 1f;
            return 1f - remaining / RegenIntervalSec;
        }
    }
}
