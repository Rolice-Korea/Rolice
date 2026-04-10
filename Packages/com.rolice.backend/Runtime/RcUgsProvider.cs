namespace Rolice.System.Backend
{
    public sealed class RcUgsProvider : IBackendProvider
    {
        public IAuthService      Auth      { get; } = new RcUgsAuthService();
        public ICloudSyncService CloudSync { get; } = new RcCloudSaveService();
    }
}
