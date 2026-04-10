using Rolice.System.Backend;

namespace Rolice.System.Backend
{
    public sealed class RcFirebaseProvider : IBackendProvider
    {
        public IAuthService      Auth      { get; } = new RcFirebaseAuthService();
        public ICloudSyncService CloudSync { get; } = new NullCloudSyncServiceImpl();
    }

    // Firebase Firestore 연동 전까지 임시 — 로컬 저장만 사용
    internal sealed class NullCloudSyncServiceImpl : ICloudSyncService
    {
        public Cysharp.Threading.Tasks.UniTask SaveAsync(string key, string json) =>
            Cysharp.Threading.Tasks.UniTask.CompletedTask;

        public Cysharp.Threading.Tasks.UniTask<string> LoadAsync(string key) =>
            Cysharp.Threading.Tasks.UniTask.FromResult<string>(null);
    }
}
