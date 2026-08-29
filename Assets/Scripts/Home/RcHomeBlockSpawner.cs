using System.Collections.Generic;
using Rolice.Data;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 Presentation 레이어 — RcHomeBuildManager의 배치/제거 이벤트를 구독해
    /// 블록 GameObject를 스폰/디스폰한다. 도메인 모델은 건드리지 않고 반응만 한다.
    ///
    /// 구독 생명주기: OnEnable에서 구독, OnDisable에서 해제(매니저는 씬을 넘어 살아있는
    /// 싱글톤이므로 해제하지 않으면 파괴된 뷰로 콜백이 새어 들어온다).
    /// </summary>
    public class RcHomeBlockSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject blockPrefab;

        [Tooltip("블록이 담길 부모. 섬 루트 하위여야 섬과 함께 움직인다.")]
        [SerializeField] private Transform blocksParent;

        private readonly Dictionary<Vector3Int, GameObject> views = new();

        private void OnEnable()
        {
            var manager = RcHomeBuildManager.Instance;
            manager.OnBlockPlaced  += HandleBlockPlaced;
            manager.OnBlockRemoved += HandleBlockRemoved;
        }

        private void OnDisable()
        {
            var manager = RcHomeBuildManager.Instance;
            manager.OnBlockPlaced  -= HandleBlockPlaced;
            manager.OnBlockRemoved -= HandleBlockRemoved;
        }

        private void HandleBlockPlaced(RcPlacedBlock block)
        {
            if (blockPrefab == null)
            {
                Debug.LogWarning("[HomeBlockSpawner] blockPrefab이 지정되지 않았습니다.");
                return;
            }

            if (views.ContainsKey(block.Position))
                return;

            // 섬 로컬 배치: 섬이 로비에서 떠다니고 회전해도 블록이 함께 따라가야 한다.
            // (월드 좌표로 Instantiate하면 부모의 이동/회전이 반영되지 않아 어긋난다.)
            GameObject view = Instantiate(blockPrefab, blocksParent);
            view.transform.localPosition = RcHomeGrid.GridToWorld(block.Position);
            view.transform.localRotation = Quaternion.identity;
            view.name = $"Block_{block.Position.x}_{block.Position.y}_{block.Position.z}";

            // 배치 타겟터가 면 인접 배치를 계산할 수 있도록 그리드 좌표를 마커에 기록
            var tag = view.GetComponent<RcHomeBlockTag>();
            if (tag == null) tag = view.AddComponent<RcHomeBlockTag>();
            tag.Set(block.Position);

            views[block.Position] = view;

            // TODO: BlockId별 프리팹/사이즈 분기, SkinId 머티리얼 적용
        }

        private void HandleBlockRemoved(Vector3Int position)
        {
            if (!views.TryGetValue(position, out GameObject view))
                return;

            if (view != null)
                Destroy(view);

            views.Remove(position);
        }
    }
}
