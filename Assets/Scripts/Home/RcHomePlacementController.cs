using UnityEngine;
using UnityEngine.EventSystems;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 배치 인터랙션 오케스트레이터. 포인터 ray로 타겟을 해석하고, 커서를 갱신하며,
    /// 좌클릭 배치 / 우클릭 제거를 도메인 매니저(RcHomeBuildManager)에 커밋한다.
    /// 커밋 후의 시각 반영(스폰/디스폰)은 매니저 이벤트 → RcHomeBlockSpawner 경로로 일어난다.
    ///
    /// 입력은 프로젝트 컨벤션(RcLobbyCube)을 따라 레거시 Input + Physics.Raycast 사용.
    /// </summary>
    public class RcHomePlacementController : MonoBehaviour
    {
        [Header("의존")]
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;
        [SerializeField] private RcHomeBlockCursor cursor;

        [Tooltip("그리드 원점이 되는 섬 루트. 비우면 월드 원점 기준.")]
        [SerializeField] private Transform islandRoot;

        [Header("현재 선택 (추후 인벤토리 UI가 주입)")]
        [SerializeField] private int    currentBlockId;
        [SerializeField] private string currentSkinId;

        private RcHomePlacementTargeter targeter;

        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;

        private void Awake()
        {
            targeter = new RcHomePlacementTargeter(islandRoot);
        }

        private void Update()
        {
            // UI 위에서는 배치/제거 차단(인벤토리 등 클릭 보호)
            if (IsPointerOverUI())
            {
                cursor?.Hide();
                return;
            }

            var cam = Cam;
            if (cam == null)
            {
                cursor?.Hide();
                return;
            }

            RcPlacementHit hit = targeter.Resolve(cam.ScreenPointToRay(Input.mousePosition));

            // 좌표 유효성(타겟터) + 재고 유효성(인벤토리)을 합쳐야 커서 색이 실제 배치 결과와 일치한다.
            bool canPlace = hit.CanPlace && HasStock();

            UpdateCursor(hit, canPlace);
            HandleInput(hit, canPlace);
        }

        /// <summary>인벤토리 미배선(개발용 씬)이면 제약 없음으로 본다 — RcHomeBuildManager와 동일 규약.</summary>
        private bool HasStock()
        {
            var inventory = RcHomeBuildManager.Instance.Inventory;
            return inventory == null || inventory.CanPlace(currentBlockId);
        }

        private void UpdateCursor(RcPlacementHit hit, bool canPlace)
        {
            if (cursor == null) return;

            if (!hit.HasHit)
                cursor.Hide();
            else
                cursor.Show(hit.PlaceCell, canPlace);
        }

        private void HandleInput(RcPlacementHit hit, bool canPlace)
        {
            if (!hit.HasHit) return;

            if (Input.GetMouseButtonDown(0) && canPlace)
                RcHomeBuildManager.Instance.TryPlace(hit.PlaceCell, currentBlockId, currentSkinId);
            else if (Input.GetMouseButtonDown(1) && hit.IsBlock)
                RcHomeBuildManager.Instance.Remove(hit.HitCell);
        }

        private static bool IsPointerOverUI()
            => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
