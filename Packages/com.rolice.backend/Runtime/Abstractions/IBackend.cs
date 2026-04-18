namespace Rolice.System.Backend
{
    public interface IBackend
    {
        IAuthService      Auth      { get; }
        ICloudSyncService CloudSync { get; }
        IEconomyService   Economy   { get; }
        IAdsService       Ads       { get; }
        IAdRewardStorage  AdReward  { get; }
    }
}
