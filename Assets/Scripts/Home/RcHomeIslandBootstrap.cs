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

        private readonly RcHomeSession session = new();

        /// <summary>인벤토리 팔레트 UI(S6)/상점이 참조할 현재 인벤토리.</summary>
        public IRcBlockInventory Inventory => session.Inventory;

        private void Start()
        {
            session.Begin(RcPlayerState.Instance.Data.Home, useUnlimitedInventory);
        }

        private void OnDisable()
        {
            if (!session.IsActive) return;

            RcPlayerState.Instance.SaveLocal();
            session.End();
        }
    }
}
