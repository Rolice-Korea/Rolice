using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 그리드 좌표 ↔ 월드 좌표 변환. 게임 타일과 동일하게 1유닛 셀.
    /// 로직은 Vector3Int(x,z=수평, y=상하 적층), 연출에서만 이 변환으로 월드 위치를 구한다.
    /// 순수 정수 매핑 — 바닥 정렬(프리팹 피벗) 같은 시각 오프셋은 표현 레이어가 처리한다.
    /// </summary>
    public static class RcHomeGrid
    {
        public const float CellSize = 1f;

        public static Vector3 GridToWorld(Vector3Int grid)
            => new Vector3(grid.x, grid.y, grid.z) * CellSize;

        public static Vector3Int WorldToGrid(Vector3 world)
            => new Vector3Int(
                Mathf.RoundToInt(world.x / CellSize),
                Mathf.RoundToInt(world.y / CellSize),
                Mathf.RoundToInt(world.z / CellSize));
    }
}
