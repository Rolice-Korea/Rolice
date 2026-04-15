using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public static class RcBackendServices
    {
        public static IBackendProvider  Provider  { get; private set; } = new NullBackendProvider();

        public static IAuthService      Auth      => Provider.Auth;
        public static ICloudSyncService CloudSync => Provider.CloudSync;

        // Economy는 로컬 저장 의존으로 IBackendProvider와 분리 등록
        public static IEconomyService   Economy   { get; private set; } = new NullEconomyService();

        // UGS 제거 시 이 한 줄만 주석처리
        public static void Register(IBackendProvider provider) => Provider = provider;

        public static void RegisterEconomy(IEconomyService economy) => Economy = economy;
    }

    internal sealed class NullBackendProvider : IBackendProvider
    {
        public IAuthService      Auth      { get; } = new NullAuthService();
        public ICloudSyncService CloudSync { get; } = new NullCloudSyncService();
    }

    internal sealed class NullAuthService : IAuthService
    {
        public bool IsAuthenticated => false;
        public UniTask EnsureAuthAsync() => UniTask.CompletedTask;
    }

    internal sealed class NullCloudSyncService : ICloudSyncService
    {
        public UniTask SaveAsync(string key, string json) => UniTask.CompletedTask;
        public UniTask<string> LoadAsync(string key)     => UniTask.FromResult<string>(null);
    }

    internal sealed class NullEconomyService : IEconomyService
    {
        public int  GetBalance(string currencyKey)            => 0;
        public void Add(string currencyKey, int amount)       { }
        public bool Spend(string currencyKey, int amount)     => false;
        public bool HasItem(string itemId)                    => false;
        public void AddItem(string itemId)                    { }
    }
}
