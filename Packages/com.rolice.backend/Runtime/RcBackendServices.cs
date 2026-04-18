using System;
using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public static class RcBackendServices
    {
        private static IBackend _backend = new NullBackend();

        public static IAuthService      Auth      => _backend.Auth;
        public static ICloudSyncService CloudSync => _backend.CloudSync;
        public static IEconomyService   Economy   => _backend.Economy;
        public static IAdsService       Ads       => _backend.Ads;
        public static IAdRewardStorage  AdReward  => _backend.AdReward;

        public static void Register(IBackend backend) => _backend = backend;
    }

    internal sealed class NullBackend : IBackend
    {
        public IAuthService      Auth      { get; } = new NullAuthService();
        public ICloudSyncService CloudSync { get; } = new NullCloudSyncService();
        public IEconomyService   Economy   { get; } = new NullEconomyService();
        public IAdsService       Ads       { get; } = new NullAdsService();
        public IAdRewardStorage  AdReward  { get; } = new NullAdRewardStorage();
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
        public int           GetBalance(string currencyKey)              => 0;
        public UniTask       AddAsync(string currencyKey, int amount)    => UniTask.CompletedTask;
        public UniTask<bool> SpendAsync(string currencyKey, int amount)  => UniTask.FromResult(false);
        public bool          HasItem(string itemId)                      => false;
        public UniTask       AddItemAsync(string itemId)                 => UniTask.CompletedTask;
        public UniTask       SyncFromCloudAsync()                        => UniTask.CompletedTask;
    }

    internal sealed class NullAdsService : IAdsService
    {
        public bool IsReady => false;
        public UniTask<bool> ShowRewardedAdAsync() => UniTask.FromResult(false);
    }

    internal sealed class NullAdRewardStorage : IAdRewardStorage
    {
        public UniTask RecordRewardAsync()                  => UniTask.CompletedTask;
        public UniTask<DateTime?> GetLastRewardTimeAsync() => UniTask.FromResult<DateTime?>(null);
    }
}
