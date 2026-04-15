namespace Rolice.System.Backend
{
    /// <summary>
    /// 이코노미 서비스 추상화. 재화 키는 문자열로 받아 패키지 의존성 분리.
    /// main project에서 RcCurrencyId enum 편의 API는 RcEconomyServiceExtensions로 제공.
    /// </summary>
    public interface IEconomyService
    {
        // ─── 재화 ────────────────────────────────────────────────────────────

        /// <summary>현재 보유 재화량 반환. key는 RcCurrencyId.ToKey() 결과.</summary>
        int GetBalance(string currencyKey);

        /// <summary>재화 추가. amount는 양수여야 한다.</summary>
        void Add(string currencyKey, int amount);

        /// <summary>재화 차감. 잔액 부족 시 false 반환하며 차감하지 않는다.</summary>
        bool Spend(string currencyKey, int amount);

        // ─── 아이템 ──────────────────────────────────────────────────────────

        /// <summary>아이템 보유 여부.</summary>
        bool HasItem(string itemId);

        /// <summary>아이템 추가. 이미 보유 중이면 무시.</summary>
        void AddItem(string itemId);
    }
}
