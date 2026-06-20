using Rolice.System;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 씬 셋업. 영속성(RcPlayerState)과 도메인(RcHomeBuildManager)을 배선한다.
    /// RcGameBootstrap과 동일 역할 — 씬 진입 시 저장된 레이아웃을 로드하고,
    /// 씬 종료 시 모델을 세이브에 반영한다.
    ///
    /// 순서 보장: Presentation(RcHomeBlockSpawner)은 OnEnable에서 구독하고,
    /// 본 부트스트랩은 Start에서 Load한다 → 모든 OnEnable이 Start보다 먼저 도므로
    /// 로드 시 발행되는 OnBlockPlaced를 스포너가 안전하게 받는다.
    /// </summary>
    public class RcHomeBootstrap : MonoBehaviour
    {
        [Header("Dev")]
        [Tooltip("저장된 블록이 없을 때 테스트 블록을 배치(개발용). 저장되면 영구 반영되니 주의.")]
        [SerializeField] private bool placeTestBlocksIfEmpty;

        private void Start()
        {
            var homeData = RcPlayerState.Instance.Data.Home;
            RcHomeBuildManager.Instance.Load(homeData);

            if (placeTestBlocksIfEmpty && RcHomeBuildManager.Instance.Count == 0)
                PlaceTestBlocks();
        }

        private void OnDisable()
        {
            Save();
        }

        /// <summary>현재 그리드 모델을 세이브에 반영하고 로컬에 기록한다.</summary>
        public void Save()
        {
            var state = RcPlayerState.Instance;
            RcHomeBuildManager.Instance.WriteTo(state.Data.Home);
            state.SaveLocal();
        }

        private void PlaceTestBlocks()
        {
            var manager = RcHomeBuildManager.Instance;
            manager.TryPlace(new Vector3Int(0, 0, 0), 0, null);
            manager.TryPlace(new Vector3Int(1, 0, 0), 0, null);
            manager.TryPlace(new Vector3Int(0, 1, 0), 0, null);
        }
    }
}
