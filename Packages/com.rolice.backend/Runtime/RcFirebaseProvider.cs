using Rolice.System.Backend;

namespace Rolice.System.Backend
{
    public sealed class RcFirebaseProvider : IBackendProvider
    {
        public IAuthService      Auth      { get; } = new RcFirebaseAuthService();
        public ICloudSyncService CloudSync { get; } = new RcFirestoreService();
    }

}
