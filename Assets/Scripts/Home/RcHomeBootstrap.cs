using Rolice.System;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 별도 홈 씬의 셋업. 로비 편입(RcHomeIslandBootstrap) 이후에도 <b>이터레이션 하네스</b>로 존치한다
    /// — 기획이 흔들리는 동안 배치/카메라 감각을 로비 배선 없이 굴려보기 위함.
    /// 도메인 배선은 RcHomeSession이 담당하므로 로비 경로와 로직이 갈라지지 않는다.
    ///
    /// 순서 보장: Presentation(RcHomeBlockSpawner)은 OnEnable에서 구독하고
    /// 본 부트스트랩은 Start에서 Load한다 → 모든 OnEnable이 Start보다 먼저 도므로
    /// 로드 시 발행되는 OnBlockPlaced를 스포너가 안전하게 받는다.
    /// </summary>
    public class RcHomeBootstrap : MonoBehaviour
    {
        [Header("Dev")]
        [Tooltip("저장된 블록이 없을 때 테스트 블록을 배치(개발용). 블록 3개 지급까지 함께 일어나며, 둘 다 세이브에 영구 반영되니 주의.")]
        [SerializeField] private bool placeTestBlocksIfEmpty;

        [Tooltip("재고 무시하고 무제한 배치(개발용). 획득 모델 검증 시에는 반드시 끌 것.")]
        [SerializeField] private bool useUnlimitedInventory;

        private readonly RcHomeSession session = new();

        /// <summary>인벤토리 팔레트 UI(S6)/상점이 참조할 현재 인벤토리.</summary>
        public IRcBlockInventory Inventory => session.Inventory;

        private void Start()
        {
            session.Begin(RcPlayerState.Instance.Data.Home, useUnlimitedInventory);

            if (placeTestBlocksIfEmpty && RcHomeBuildManager.Instance.Count == 0)
                PlaceTestBlocks();
        }

        private void OnDisable()
        {
            if (!session.IsActive) return;

            Save();
            session.End();
        }

        /// <summary>현재 상태를 디스크에 기록한다(DTO는 배치/제거마다 이미 갱신돼 있다).</summary>
        public void Save()
        {
            RcPlayerState.Instance.SaveLocal();
        }

        private void PlaceTestBlocks()
        {
            // 재고형에서도 동작하도록 먼저 지급한다(무한형에서는 해금만 일어남).
            session.Inventory.Grant(0, 3);

            var manager = RcHomeBuildManager.Instance;
            manager.TryPlace(new Vector3Int(0, 0, 0), 0, null);
            manager.TryPlace(new Vector3Int(1, 0, 0), 0, null);
            manager.TryPlace(new Vector3Int(0, 1, 0), 0, null);
        }
    }
}
