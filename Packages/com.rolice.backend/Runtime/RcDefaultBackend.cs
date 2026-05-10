namespace Rolice.System.Backend
{
    /// <summary>
    /// 프로젝트 기본 백엔드 구성 프리셋.
    /// Auth/CloudSync: Firebase, Economy: Firestore, Ads: Dummy, AdReward: Firestore.
    /// SDK 교체 시 이 클래스 내부만 수정한다.
    /// </summary>
    public sealed class RcDefaultBackend : IBackend
    {
        public IAuthService      Auth      { get; }
        public ICloudSyncService CloudSync { get; }
        public IEconomyService   Economy   { get; }
        public IAdsService       Ads       { get; }
        public IAdRewardStorage  AdReward  { get; }

        public RcDefaultBackend(IPlayerDataCache playerData)
        {
            Auth      = new RcFirebaseAuthService();
            CloudSync = new RcFirestoreService();
            Economy   = new RcFirestoreEconomyService(playerData);
            Ads       = new RcAdMobAdsService();
            AdReward  = new RcFirestoreAdRewardStorage();
        }
    }
}
