namespace Rolice.System.Backend
{
    public interface IBackendProvider
    {
        IAuthService      Auth      { get; }
        ICloudSyncService CloudSync { get; }
        // 추후: IEconomyService Economy { get; }
    }
}
