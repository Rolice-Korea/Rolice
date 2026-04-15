using Rolice.Define;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.System.Economy
{
    /// <summary>
    /// 로컬 JSON 저장 기반 이코노미 구현체.
    /// RcPlayerState가 소유한 RcPlayerData를 읽고 쓰며, 변경 후 즉시 로컬 저장.
    /// 데이터 소유권은 RcPlayerState에 있음 — 직접 인스턴스를 생성하지 않는다.
    /// </summary>
    public sealed class RcLocalEconomyService : IEconomyService
    {
        private readonly RcPlayerState playerState;

        public RcLocalEconomyService(RcPlayerState playerState)
        {
            this.playerState = playerState;
        }

        // ─── 재화 ────────────────────────────────────────────────────────────

        public int GetBalance(string currencyKey)
        {
            return playerState.Data.GetCurrency(currencyKey);
        }

        public void Add(string currencyKey, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[Economy] Add: amount는 양수여야 합니다. key={currencyKey}, amount={amount}");
                return;
            }

            var data    = playerState.Data;
            int current = data.GetCurrency(currencyKey);
            data.SetCurrency(currencyKey, current + amount);
            playerState.SaveLocal();
        }

        public bool Spend(string currencyKey, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[Economy] Spend: amount는 양수여야 합니다. key={currencyKey}, amount={amount}");
                return false;
            }

            var data    = playerState.Data;
            int current = data.GetCurrency(currencyKey);

            if (current < amount)
                return false;

            data.SetCurrency(currencyKey, current - amount);
            playerState.SaveLocal();
            return true;
        }

        // ─── 아이템 ──────────────────────────────────────────────────────────

        public bool HasItem(string itemId)
        {
            return playerState.Data.HasOwnedItem(itemId);
        }

        public void AddItem(string itemId)
        {
            playerState.Data.AddOwnedItem(itemId);
            playerState.SaveLocal();
        }
    }

    /// <summary>
    /// RcCurrencyId enum 편의 확장. IEconomyService 인터페이스는 string 기반이므로
    /// 패키지 의존성 없이 main project에서 enum → key 변환을 담당한다.
    /// </summary>
    public static class RcEconomyServiceExtensions
    {
        public static int  GetBalance(this IEconomyService svc, RcCurrencyId id)
            => svc.GetBalance(id.ToKey());

        public static void Add(this IEconomyService svc, RcCurrencyId id, int amount)
            => svc.Add(id.ToKey(), amount);

        public static bool Spend(this IEconomyService svc, RcCurrencyId id, int amount)
            => svc.Spend(id.ToKey(), amount);
    }
}
