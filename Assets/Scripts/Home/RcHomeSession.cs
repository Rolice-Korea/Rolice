using Rolice.Data;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 도메인 배선의 수명 관리 — 세이브 DTO에 인벤토리와 그리드 모델을 물리고, 끝나면 끊는다.
    ///
    /// 별도 홈 씬(RcHomeBootstrap)과 로비 편입(RcHomeIslandBootstrap)이 같은 배선을 필요로 하는데,
    /// 부트스트랩마다 복붙하면 한쪽만 고치는 사고가 난다. MonoBehaviour가 아니라 순수 C#이라
    /// 하네스에서 그대로 검증할 수 있다.
    ///
    /// 영속성은 다루지 않는다(RcPlayerState 미참조). 매니저가 배치/제거마다 DTO를 갱신하므로
    /// 디스크 기록은 호출자가 원하는 시점에 SaveLocal 하면 된다.
    /// </summary>
    public class RcHomeSession
    {
        private RcHomeData data;

        /// <summary>현재 세션의 인벤토리. 팔레트 UI/상점이 참조한다.</summary>
        public IRcBlockInventory Inventory { get; private set; }

        public bool IsActive => data != null;

        /// <summary>
        /// 세이브 DTO를 물고 도메인을 활성화한다.
        /// </summary>
        /// <param name="homeData">RcPlayerState.Data.Home</param>
        /// <param name="unlimited">개발용 — 재고 무시(무한형 구현 주입)</param>
        public void Begin(RcHomeData homeData, bool unlimited = false)
        {
            if (homeData == null)
                return;

            data      = homeData;
            Inventory = unlimited
                ? new RcUnlimitedBlockInventory(homeData)
                : (IRcBlockInventory)new RcCountedBlockInventory(homeData);

            var manager = RcHomeBuildManager.Instance;
            manager.Inventory = Inventory;
            manager.Load(homeData);
        }

        /// <summary>
        /// 배선을 끊는다. 매니저는 순수 C# 싱글톤이라 씬보다 오래 살기 때문에,
        /// 끊지 않으면 씬을 벗어난 뒤의 유령 배치가 재고 제약 없이 실제 세이브를 건드린다.
        /// </summary>
        public void End()
        {
            var manager = RcHomeBuildManager.Instance;
            manager.Clear();
            manager.Inventory = null;

            Inventory = null;
            data      = null;
        }
    }
}
