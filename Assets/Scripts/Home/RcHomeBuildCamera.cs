using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 홈 편집용 빌더 카메라. 포커스 지점을 중심으로 팬/줌/요(yaw) 회전한다.
    /// 좌/우클릭은 배치·제거에 쓰이므로 카메라 조작은 중간버튼 드래그·휠·키보드(WASD/QE)로 분리.
    /// 피치는 고정(인스펙터)이라 블록 옆면을 보며 쌓을 수 있다. 목표값을 Lerp로 보간해 부드럽게 추종.
    ///
    /// 입력은 프로젝트 컨벤션(레거시 Input)을 따른다. 완성 후 3인칭 카메라는 별도 컴포넌트로
    /// 추가하고 상위에서 모드 전환할 예정(지금은 단일 모드라 추상화하지 않음).
    /// </summary>
    public class RcHomeBuildCamera : MonoBehaviour
    {
        [Header("기준")]
        [SerializeField] private Vector3 focusPoint = Vector3.zero;
        [SerializeField] private float   pitch = 45f;   // 내려다보는 각도(고정)
        [SerializeField] private float   yaw   = 45f;   // 시작 방위

        [Header("거리(줌)")]
        [SerializeField] private float distance    = 12f;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private float zoomSpeed    = 4f;

        [Header("팬")]
        [SerializeField] private float   keyPanSpeed  = 8f;     // WASD (유닛/초)
        [SerializeField] private float   dragPanSpeed = 0.02f;  // 중간드래그 1px당 (거리 비례)
        [SerializeField] private Vector2 panLimitX    = new(-25f, 25f);
        [SerializeField] private Vector2 panLimitZ    = new(-25f, 25f);

        [Header("회전")]
        [SerializeField] private float yawSpeed = 90f;  // Q/E (도/초)

        [Header("부드러움")]
        [SerializeField] private float smooth = 12f;

        private Vector3 targetFocus;
        private float   targetDistance;
        private float   targetYaw;
        private Vector3 lastMousePos;

        private void Awake()
        {
            targetFocus    = focusPoint;
            targetDistance = distance;
            targetYaw      = yaw;
        }

        private void Update()
        {
            HandleZoom();
            HandleRotate();
            HandlePan();
        }

        private void LateUpdate()
        {
            float t = smooth * Time.deltaTime;
            focusPoint = Vector3.Lerp(focusPoint, targetFocus, t);
            distance   = Mathf.Lerp(distance, targetDistance, t);
            yaw        = Mathf.LerpAngle(yaw, targetYaw, t);

            ApplyTransform();
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
                targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
        }

        private void HandleRotate()
        {
            float dir = 0f;
            if (Input.GetKey(KeyCode.Q)) dir += 1f;
            if (Input.GetKey(KeyCode.E)) dir -= 1f;
            if (dir != 0f)
                targetYaw += dir * yawSpeed * Time.deltaTime;
        }

        private void HandlePan()
        {
            Vector3 move = Vector3.zero;

            float h = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
            float v = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);
            if (h != 0f || v != 0f)
                move += PlanarDir(h, v) * (keyPanSpeed * Time.deltaTime);

            if (Input.GetMouseButtonDown(2))
            {
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                lastMousePos = Input.mousePosition;
                // 화면을 끌어당기는 느낌 → 드래그 반대 방향으로 월드 이동, 거리에 비례
                move += PlanarDir(-delta.x, -delta.y) * (dragPanSpeed * targetDistance);
            }

            if (move == Vector3.zero) return;

            targetFocus += move;
            targetFocus.x = Mathf.Clamp(targetFocus.x, panLimitX.x, panLimitX.y);
            targetFocus.z = Mathf.Clamp(targetFocus.z, panLimitZ.x, panLimitZ.y);
        }

        // 카메라 yaw 기준 수평 이동 방향 (right, forward → 월드 벡터)
        private Vector3 PlanarDir(float right, float forward)
        {
            float rad = targetYaw * Mathf.Deg2Rad;
            Vector3 fwd   = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            Vector3 rightV = new Vector3(fwd.z, 0f, -fwd.x);
            return rightV * right + fwd * forward;
        }

        private void ApplyTransform()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = rot * Vector3.forward;
            transform.SetPositionAndRotation(focusPoint - dir * distance, rot);
        }
    }
}
