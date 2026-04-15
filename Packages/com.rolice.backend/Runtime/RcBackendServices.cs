using System;
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

        // 광고·보상 이력은 SDK/스토리지 구현체가 결정되기 전까지 분리 등록
        public static IAdsService       Ads       { get; private set; } = new NullAdsService();
        public static IAdRewardStorage  AdReward  { get; private set; } = new NullAdRewardStorage();

        // UGS 제거 시 이 한 줄만 주석처리
        public static void Register(IBackendProvider provider) => Provider = provider;

        public static void RegisterEconomy(IEconomyService economy) => Economy = economy;
        public static void RegisterAds(IAdsService ads)             => Ads = ads;
        public static void RegisterAdReward(IAdRewardStorage storage) => AdReward = storage;
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
        public int           GetBalance(string currencyKey)           => 0;
        public UniTask       AddAsync(string currencyKey, int amount) => UniTask.CompletedTask;
        public UniTask<bool> SpendAsync(string currencyKey, int amount) => UniTask.FromResult(false);
        public bool          HasItem(string itemId)                   => false;
        public UniTask       AddItemAsync(string itemId)              => UniTask.CompletedTask;
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
