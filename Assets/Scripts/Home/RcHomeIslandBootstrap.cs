using Rolice.System;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 로비에 편입된 섬의 도메인 배선. 로비 진입 시 저장된 레이아웃을 물리고, 이탈 시 끊는다.
    /// 배선 자체는 RcHomeSession이 담당하므로 별도 홈 씬(RcHomeBootstrap)과 로직이 갈라지지 않는다.
    ///
    /// RcLobbyBootstrap과 분리한 이유: 로비 부트스트랩은 UI 패널 오픈 책임이고
    /// 여기는 홈 도메인 수명 책임이라 바뀌는 이유가 다르다(SRP).
    /// 실행 순서에 의존하지 않는다 — 스포너는 OnEnable에서 구독하고 여기는 Start에서 Load한다.
    /// </summary>
    public class RcHomeIslandBootstrap : MonoBehaviour
    {
        [Header("Dev")]
        [Tooltip("재고 무시하고 무제한 배치(개발용). 획득 모델 검증 시에는 반드시 끌 것.")]
        [SerializeField] private bool useUnlimitedInventory;

        [Tooltip("개발용 시작 해금 블록 ID. 무한형도 '해금된 종류'만 놓을 수 있으므로 이게 비면 " +
                 "아무것도 배치되지 않는다. 클리어 보상/상점이 붙으면 비울 것 — 세이브에 영구 반영된다.")]
        [SerializeField] private int[] devUnlockBlockIds = { 0 };

        [Tooltip("개수형일 때 위 블록을 몇 개씩 지급할지. 0이면 해금만 하고 개수는 안 준다.")]
        [SerializeField] private int devGrantCount = 20;

        private readonly RcHomeSession session = new();

        /// <summary>인벤토리 팔레트 UI(S6)/상점이 참조할 현재 인벤토리.</summary>
        public IRcBlockInventory Inventory => session.Inventory;

        private void Start()
        {
            session.Begin(RcPlayerState.Instance.Data.Home, useUnlimitedInventory);
            ApplyDevUnlocks();
        }

        /// <summary>
        /// 블록을 버는 경로(클리어 보상/상점)가 아직 없어서, 그것 없이 배치를 시험하기 위한 개발용 지급.
        /// 무한형도 CanPlace가 IsUnlocked를 보므로 해금 없이는 무한형조차 아무것도 못 놓는다.
        /// </summary>
        private void ApplyDevUnlocks()
        {
            if (devUnlockBlockIds == null || devUnlockBlockIds.Length == 0)
                return;

            foreach (int blockId in devUnlockBlockIds)
            {
                if (devGrantCount > 0) session.Inventory.Grant(blockId, devGrantCount);
                else                   session.Inventory.Unlock(blockId);
            }

            Debug.Log($"[HomeIslandBootstrap] 개발용 해금 {devUnlockBlockIds.Length}종 적용 " +
                      $"(개당 {devGrantCount}개). 세이브에 영구 반영됨.");
        }

        private void OnDisable()
        {
            if (!session.IsActive) return;

            RcPlayerState.Instance.SaveLocal();
            session.End();
        }
    }
}
