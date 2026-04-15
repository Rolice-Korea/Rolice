using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    /// <summary>
    /// 이코노미 서비스 추상화.
    /// 읽기(GetBalance, HasItem)는 로컬 캐시 기반(동기).
    /// 쓰기(AddAsync, SpendAsync, AddItemAsync)는 서버 권위(비동기) — 실패 시 예외.
    /// </summary>
    public interface IEconomyService
    {
        // ─── 재화 (Read: 로컬 캐시 / Write: 서버 권위) ───────────────────────

        /// <summary>현재 보유 재화량. 로컬 캐시에서 읽음.</summary>
        int GetBalance(string currencyKey);

        /// <summary>재화 추가. Firestore atomic increment. 실패 시 예외.</summary>
        UniTask AddAsync(string currencyKey, int amount);

        /// <summary>
        /// 재화 차감. Firestore 트랜잭션으로 잔액 검증 + 차감.
        /// 잔액 부족 시 false 반환. 서버 오류 시 예외.
        /// </summary>
        UniTask<bool> SpendAsync(string currencyKey, int amount);

        // ─── 아이템 (Read: 로컬 캐시 / Write: 서버 권위) ─────────────────────

        /// <summary>아이템 보유 여부. 로컬 캐시에서 읽음.</summary>
        bool HasItem(string itemId);

        /// <summary>아이템 추가. Firestore merge. 실패 시 예외.</summary>
        UniTask AddItemAsync(string itemId);
    }
}
