using System;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 블록 보유 상태의 추상. 배치 로직(RcHomeBuildManager)이 재고 규칙을 모른 채
    /// CanPlace/OnPlaced/OnRemoved만 호출하게 해서, 획득 모델이 바뀌어도 구현체 교체로 끝나게 한다.
    ///
    /// 현재 채택 모델: 종류 해금 + 개수 보유 + 제거 시 반환(RcCountedBlockInventory).
    /// 뒤집힘 대비: 종류만 해금하고 무제한 사용(RcUnlimitedBlockInventory).
    /// </summary>
    public interface IRcBlockInventory
    {
        /// <summary>해금/개수가 변해 UI 갱신이 필요함.</summary>
        event Action OnChanged;

        /// <summary>
        /// 개수를 추적하는 모델인가. 팔레트 UI가 개수 뱃지를 그릴지 판단한다
        /// (무한형에서 GetCount는 int.MaxValue라 그대로 그리면 안 됨).
        /// </summary>
        bool TracksCount { get; }

        /// <summary>해당 블록 종류를 해금했는가(팔레트 노출 기준).</summary>
        bool IsUnlocked(int blockId);

        /// <summary>배치 가능한 잔여 개수. 무제한 구현은 int.MaxValue를 돌려준다.</summary>
        int GetCount(int blockId);

        /// <summary>지금 1개 배치할 수 있는가(해금 + 잔여 개수).</summary>
        bool CanPlace(int blockId);

        /// <summary>배치가 커밋됨 — 재고에서 차감.</summary>
        void OnPlaced(int blockId);

        /// <summary>제거가 커밋됨 — 재고로 반환(소실 없음).</summary>
        void OnRemoved(int blockId);

        /// <summary>종류 해금(개수는 그대로).</summary>
        void Unlock(int blockId);

        /// <summary>지급 — 해금 + 개수 가산. 클리어 보상/상점 구매의 공통 진입점.</summary>
        void Grant(int blockId, int count);
    }
}
