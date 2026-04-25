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

        public static void Register(IBackend backend)               => _backend = backend;
        public static void RegisterOffline(bool alwaysSucceed = true) => _backend = new NullBackend(alwaysSucceed);
    }

    internal sealed class NullBackend : IBackend
    {
        public IAuthService      Auth      { get; }
        public ICloudSyncService CloudSync { get; }
        public IEconomyService   Economy   { get; }
        public IAdsService       Ads       { get; }
        public IAdRewardStorage  AdReward  { get; }

        public NullBackend(bool alwaysSucceed = false)
        {
            Auth      = new NullAuthService(alwaysSucceed);
            CloudSync = new NullCloudSyncService();
            Economy   = new NullEconomyService(alwaysSucceed);
            Ads       = new NullAdsService(alwaysSucceed);
            AdReward  = new NullAdRewardStorage();
        }
    }

    internal sealed class NullAuthService : IAuthService
    {
        private readonly bool alwaysSucceed;
        public NullAuthService(bool alwaysSucceed) => this.alwaysSucceed = alwaysSucceed;

        public bool IsAuthenticated => alwaysSucceed;
        public UniTask EnsureAuthAsync() => UniTask.CompletedTask;
        public UniTask SignInAsync()      => UniTask.CompletedTask;
    }

    internal sealed class NullCloudSyncService : ICloudSyncService
    {
        public UniTask SaveAsync(string key, string json) => UniTask.CompletedTask;
        public UniTask<string> LoadAsync(string key)     => UniTask.FromResult<string>(null);
    }

    internal sealed class NullEconomyService : IEconomyService
    {
        private readonly bool alwaysSucceed;
        public NullEconomyService(bool alwaysSucceed) => this.alwaysSucceed = alwaysSucceed;

        public int           GetBalance(string currencyKey)             => 0;
        public UniTask       AddAsync(string currencyKey, int amount)   => UniTask.CompletedTask;
        public UniTask<bool> SpendAsync(string currencyKey, int amount) => UniTask.FromResult(alwaysSucceed);
        public bool          HasItem(string itemId)                     => false;
        public UniTask       AddItemAsync(string itemId)                => UniTask.CompletedTask;
        public UniTask       SyncFromCloudAsync()                       => UniTask.CompletedTask;
    }

    internal sealed class NullAdsService : IAdsService
    {
        private readonly bool alwaysSucceed;
        public NullAdsService(bool alwaysSucceed) => this.alwaysSucceed = alwaysSucceed;

        public bool IsReady => alwaysSucceed;
        public UniTask<bool> ShowRewardedAdAsync() => UniTask.FromResult(alwaysSucceed);
    }

    internal sealed class NullAdRewardStorage : IAdRewardStorage
    {
        public UniTask RecordRewardAsync()                  => UniTask.CompletedTask;
        public UniTask<DateTime?> GetLastRewardTimeAsync() => UniTask.FromResult<DateTime?>(null);
    }
}
