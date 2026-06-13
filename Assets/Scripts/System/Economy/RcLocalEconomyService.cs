using Cysharp.Threading.Tasks;
using Rolice.Define;
using Rolice.System.Backend;

namespace Rolice.System.Economy
{
    /// <summary>
    /// RcCurrencyId enum 편의 확장.
    /// IEconomyService 인터페이스는 string 기반이므로
    /// 패키지 의존성 없이 main project에서 enum → key 변환을 담당한다.
    /// </summary>
    public static class RcEconomyServiceExtensions
    {
        public static int GetBalance(this IEconomyService svc, RcCurrencyId id)
            => svc.GetBalance(id.ToKey());

        public static UniTask AddAsync(this IEconomyService svc, RcCurrencyId id, int amount)
            => svc.AddAsync(id.ToKey(), amount);

        public static UniTask<bool> SpendAsync(this IEconomyService svc, RcCurrencyId id, int amount)
            => svc.SpendAsync(id.ToKey(), amount);
    }
}
