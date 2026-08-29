using System;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 로비 뷰에서의 섬 표현 — 천천히 자전하며 떠 있고, 누르면 편집 모드 진입을 요청한다.
    /// 로비 중앙의 다이스(RcLobbyCube) 자리를 대체한다.
    ///
    /// RcLobbyCube와 결정적으로 다른 점: <b>yaw 회전만 한다.</b> 자유 6DOF로 뒤집으면
    /// 섬의 위/아래가 뒤집혀 편집 좌표계(바닥 y=0, 위로 적층)와 충돌한다.
    ///
    /// 편집 중에는 SetIdle(false)로 자전·부유·입력을 모두 멈춘다.
    /// 섬이 움직이면 오비트 카메라 기준이 흔들리고, 좌클릭이 배치 입력과 겹친다.
    /// </summary>
    public class RcHomeIslandDisplay : MonoBehaviour
    {
        [Header("자동 회전")]
        [SerializeField] private float autoRotateSpeed = 8f;

        [Header("드래그 회전 (yaw 전용)")]
        [SerializeField] private float dragSensitivity   = 0.2f;
        [SerializeField] private float inertiaDecaySpeed = 2f;
        [SerializeField] private float minInertiaSpeed   = 0.1f;

        [Header("플로팅 연동")]
        [SerializeField] private RcTweenAnimator floatAnimator;

        [Header("탭 판정")]
        [Tooltip("이 픽셀 이상 끌면 회전으로 보고 클릭으로 치지 않는다.")]
        [SerializeField] private float clickThresholdPixels = 10f;

        /// <summary>섬을 눌렀다(끌지 않고). 모드 컨트롤러가 편집 진입에 사용.</summary>
        public event Action OnClicked;

        private Camera  mainCamera;
        private bool    isIdle = true;
        private bool    isDragging;
        private bool    isFloatPaused;
        private Vector2 pressPos;
        private Vector2 lastPointerPos;
        private float   yawVelocity;

        private void Awake()
        {
            mainCamera = Camera.main;

            // 클릭 판정은 자식 콜라이더(섬 메시)로도 성립하므로 자식까지 본다.
            // 루트에 별도 콜라이더를 두면 그게 배치 레이캐스트를 가로채므로 오히려 두지 않는 게 맞다.
            if (GetComponentInChildren<Collider>() == null)
                Debug.LogWarning("[HomeIslandDisplay] Collider가 없어 클릭을 받을 수 없습니다.");
        }

        /// <summary>
        /// 로비 뷰(true) / 편집 뷰(false) 전환. 편집 중에는 완전히 정지한다.
        /// 같은 값으로 다시 불러도 상태를 그대로 적용한다 — 초기 상태를 모드 컨트롤러가
        /// 한 곳에서 확정할 수 있어야 하므로(필드 초기값과 어긋난 채 첫 프레임이 도는 것 방지).
        /// </summary>
        public void SetIdle(bool idle)
        {
            isIdle = idle;

            if (!idle)
            {
                isDragging  = false;
                yawVelocity = 0f;
            }

            SetFloatPaused(!idle);
        }

        private void Update()
        {
            if (!isIdle) return;

            HandleInput();
            UpdateRotation();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
                BeginDrag();
            else if (Input.GetMouseButtonUp(0))
                EndDrag();

            if (isDragging)
                UpdateDrag();
        }

        private void BeginDrag()
        {
            if (mainCamera == null) return;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || !hit.transform.IsChildOf(transform))
                return;

            isDragging     = true;
            pressPos       = Input.mousePosition;
            lastPointerPos = pressPos;
            yawVelocity    = 0f;
            SetFloatPaused(true);
        }

        private void EndDrag()
        {
            if (!isDragging) return;

            isDragging = false;

            // 거의 안 끌었으면 탭으로 해석 — 편집 진입.
            if (Vector2.Distance(pressPos, (Vector2)Input.mousePosition) <= clickThresholdPixels)
            {
                yawVelocity = 0f;
                OnClicked?.Invoke();
            }
        }

        private void UpdateDrag()
        {
            Vector2 current = Input.mousePosition;
            yawVelocity    = -(current.x - lastPointerPos.x) * dragSensitivity;
            lastPointerPos = current;
        }

        private void UpdateRotation()
        {
            if (isDragging)
            {
                RotateYaw(yawVelocity);
                return;
            }

            if (Mathf.Abs(yawVelocity) > minInertiaSpeed)
            {
                RotateYaw(yawVelocity);
                yawVelocity = Mathf.Lerp(yawVelocity, 0f, inertiaDecaySpeed * Time.deltaTime);
                return;
            }

            yawVelocity = 0f;
            SetFloatPaused(false);
            RotateYaw(autoRotateSpeed * Time.deltaTime);
        }

        private void RotateYaw(float degrees)
        {
            transform.Rotate(Vector3.up, degrees, Space.World);
        }

        private void SetFloatPaused(bool paused)
        {
            if (floatAnimator == null) return;
            if (isFloatPaused == paused) return;

            if (paused) floatAnimator.Pause();
            else floatAnimator.Resume();

            isFloatPaused = paused;
        }
    }
}
