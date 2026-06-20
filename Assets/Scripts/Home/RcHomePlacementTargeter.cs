using UnityEngine;

namespace Rolice.Home
{
    /// <summary>포인터 ray 해석 결과 — 배치/제거에 필요한 셀 정보.</summary>
    public struct RcPlacementHit
    {
        public bool       HasHit;    // 무언가를 맞췄는가
        public bool       IsBlock;   // 블록을 맞춤(true) vs 바닥(false)
        public Vector3Int HitCell;   // 맞은 블록(또는 바닥)의 셀 — 제거 대상
        public Vector3Int PlaceCell; // 새 블록이 들어갈 셀 — 면 인접(블록) 또는 바닥 셀
        public bool       CanPlace;  // PlaceCell이 비어 있는가
    }

    /// <summary>
    /// ray → 배치/제거 타겟 해석. 블록 면을 맞으면 노멀 방향 인접 셀(마크식),
    /// 바닥을 맞으면 해당 수평 셀(높이 0)을 배치 대상으로 삼는다.
    ///
    /// 구체 클래스로 둔다 — 타겟팅 방식은 (바닥+면) 하나로 진화하지 여러 전략이
    /// 공존하지 않으므로, 인터페이스 추출은 2번째 구현이 실제로 생길 때 한다.
    /// </summary>
    public class RcHomePlacementTargeter
    {
        private const float MaxDistance = 1000f;

        public RcPlacementHit Resolve(Ray ray)
        {
            var result = new RcPlacementHit();

            if (!Physics.Raycast(ray, out RaycastHit hit, MaxDistance))
                return result;

            result.HasHit = true;

            var tag = hit.collider.GetComponentInParent<RcHomeBlockTag>();
            if (tag != null)
            {
                // 블록 면: 노멀 방향으로 한 칸 인접
                result.IsBlock   = true;
                result.HitCell   = tag.Cell;
                result.PlaceCell = tag.Cell + Vector3Int.RoundToInt(hit.normal);
            }
            else
            {
                // 바닥: 맞은 지점의 수평 셀, 높이는 0
                Vector3Int cell = RcHomeGrid.WorldToGrid(hit.point);
                cell.y = 0;
                result.IsBlock   = false;
                result.HitCell   = cell;
                result.PlaceCell = cell;
            }

            result.CanPlace = !RcHomeBuildManager.Instance.HasBlock(result.PlaceCell);
            return result;
        }
    }
}
