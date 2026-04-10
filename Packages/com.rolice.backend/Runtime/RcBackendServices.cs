using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public static class RcBackendServices
    {
        public static IBackendProvider  Provider  { get; private set; } = new NullBackendProvider();

        public static IAuthService      Auth      => Provider.Auth;
        public static ICloudSyncService CloudSync => Provider.CloudSync;

        // UGS 제거 시 이 한 줄만 주석처리
        public static void Register(IBackendProvider provider) => Provider = provider;
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
}
