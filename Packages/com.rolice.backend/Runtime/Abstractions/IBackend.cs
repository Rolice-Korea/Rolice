namespace Rolice.System.Backend
{
    public interface IBackend
    {
        IAuthService        Auth       { get; }
        ICloudSyncService   CloudSync  { get; }
        IEconomyService     Economy    { get; }
        IHeartRegenService  HeartRegen { get; }
        IAdsService         Ads        { get; }
        IAdRewardStorage    AdReward   { get; }
    }
}
