using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 스폰된 블록 GameObject에 부착되어 자신의 그리드 좌표를 기억한다.
    /// 배치 타겟터가 raycast 히트를 "블록(마커 존재) vs 바닥"으로 판별하고,
    /// 블록의 셀 + 면 노멀로 인접 셀을 계산하는 데 사용된다.
    /// </summary>
    public class RcHomeBlockTag : MonoBehaviour
    {
        public Vector3Int Cell { get; private set; }

        public void Set(Vector3Int cell) => Cell = cell;
    }
}
