using DG.Tweening;
using Rolice.System;
using Engine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.Home
{
    public enum RcHomeViewMode
    {
        Lobby,          // 섬이 떠서 자전 — 로비 UI 표시
        Transitioning,  // 카메라 전환 중 — 모든 입력 차단
        Edit,           // 블록 배치/제거 — 편집 카메라
    }

    /// <summary>
    /// 로비 ↔ 섬 편집 모드 전환. 결정 #1(심리스 = 씬 로드 없음)의 구현체로,
    /// 씬을 갈아타지 않고 <b>카메라 자세 + UI + 입력 소유권</b>만 바꾼다.
    ///
    /// 전환 중(Transitioning)에는 배치도 섬 회전도 받지 않는다. 트윈 도중 좌클릭이
    /// 들어오면 아직 도착하지 않은 카메라 기준으로 엉뚱한 셀에 배치되기 때문.
    ///
    /// 카메라는 하나(로비 메인)를 공유하고 RcHomeBuildCamera를 켜고 끈다. 전환 트윈의
    /// 도착점을 그 컴포넌트의 목표 자세로 잡아, 켜지는 순간 이음새가 생기지 않게 한다.
    /// </summary>
    public class RcHomeModeController : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private RcHomeIslandDisplay        island;
        [SerializeField] private RcHomeBuildCamera          buildCamera;
        [SerializeField] private RcHomePlacementController  placement;

        [Header("카메라")]
        [SerializeField, Tooltip("비우면 Camera.main")] private Camera targetCamera;
        [SerializeField, Tooltip("로비 뷰에서의 카메라 자세(빈 GameObject)")] private Transform lobbyCameraAnchor;
        [SerializeField] private float transitionDuration = 0.7f;
        [SerializeField] private Ease  transitionEase     = Ease.InOutCubic;

        [Header("로비 UI")]
        [Tooltip("편집 중 숨길 씬 내 UI 루트(LobbyCanvas 등). RcUIManager 캔버스 레이어에 없는 " +
                 "씬 HUD는 SetCanvasVisible로 안 숨겨지므로 여기에 넣어야 한다.")]
        [SerializeField] private GameObject[] lobbyUiRoots;

        [Header("편집 UI")]
        [Tooltip("나가기 버튼 등 편집 전용 UI 루트. 로비 캔버스와 별개여야 한다.")]
        [SerializeField] private GameObject editUiRoot;
        [SerializeField] private Button     exitButton;

        public RcHomeViewMode Mode { get; private set; } = RcHomeViewMode.Lobby;

        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;

        private Sequence transition;

        private void Awake()
        {
            // 시작은 항상 로비 뷰. 씬에 켜진 채로 저장돼 있어도 여기서 정리한다.
            // 섬 상태도 여기서 확정한다 — 컴포넌트 각자의 필드 초기값에 맡기면
            // 실행 순서에 따라 첫 프레임 상태가 갈린다.
            if (island != null)      island.SetIdle(true);
            if (buildCamera != null) buildCamera.enabled = false;
            if (placement != null)   placement.enabled   = false;
            if (editUiRoot != null)  editUiRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (island != null)     island.OnClicked += EnterEdit;
            if (exitButton != null) exitButton.onClick.AddListener(ExitEdit);
        }

        private void OnDisable()
        {
            if (island != null)     island.OnClicked -= EnterEdit;
            if (exitButton != null) exitButton.onClick.RemoveListener(ExitEdit);

            transition?.Kill();
            transition = null;
        }

        private void Update()
        {
            if (Mode == RcHomeViewMode.Edit && Input.GetKeyDown(KeyCode.Escape))
                ExitEdit();
        }

        public void EnterEdit()
        {
            if (Mode != RcHomeViewMode.Lobby) return;

            var cam = Cam;
            if (cam == null || buildCamera == null || island == null)
            {
                Debug.LogWarning("[HomeModeController] 편집 진입에 필요한 참조가 비어 있습니다.");
                return;
            }

            Mode = RcHomeViewMode.Transitioning;

            island.SetIdle(false);
            SetLobbyUiVisible(false);

            // 섬을 포커스로 잡고, 그 컴포넌트가 도달하려는 자세를 트윈 도착점으로 삼는다.
            buildCamera.SetFocus(island.transform.position);
            buildCamera.GetTargetPose(out Vector3 pos, out Quaternion rot);

            PlayTransition(cam, pos, rot, () =>
            {
                buildCamera.SetFocus(island.transform.position, snap: true);
                buildCamera.enabled = true;
                if (placement != null)  placement.enabled = true;
                if (editUiRoot != null) editUiRoot.SetActive(true);

                Mode = RcHomeViewMode.Edit;
            });
        }

        public void ExitEdit()
        {
            if (Mode != RcHomeViewMode.Edit) return;

            var cam = Cam;
            if (cam == null || lobbyCameraAnchor == null)
            {
                Debug.LogWarning("[HomeModeController] 로비 복귀에 필요한 참조가 비어 있습니다.");
                return;
            }

            Mode = RcHomeViewMode.Transitioning;

            // 편집 입력을 먼저 끊는다 — 트윈 중 카메라 조작/배치가 섞이면 안 된다.
            buildCamera.enabled = false;
            if (placement != null)  placement.enabled = false;
            if (editUiRoot != null) editUiRoot.SetActive(false);

            // 결정 #9: 디스크 기록은 편집 이탈 시점에 한 번.
            // (DTO 자체는 배치/제거마다 이미 갱신돼 있어 중간에 다른 시스템이 저장해도 안전하다.)
            RcPlayerState.Instance.SaveLocal();

            PlayTransition(cam, lobbyCameraAnchor.position, lobbyCameraAnchor.rotation, () =>
            {
                island.SetIdle(true);
                SetLobbyUiVisible(true);

                Mode = RcHomeViewMode.Lobby;
            });
        }

        /// <summary>
        /// 로비 UI 전체를 끄고 켠다. 두 경로가 필요하다 —
        /// RcUIManager가 관리하는 패널(스테이지 선택/재화 HUD)은 캔버스 레이어에 있고,
        /// 씬에 직접 놓인 HUD(LobbyCanvas 등)는 거기 없어서 따로 꺼야 한다.
        /// </summary>
        private void SetLobbyUiVisible(bool visible)
        {
            RcUIManager.Instance.SetCanvasVisible(visible);

            if (lobbyUiRoots == null) return;

            foreach (var root in lobbyUiRoots)
                if (root != null) root.SetActive(visible);
        }

        private void PlayTransition(Camera cam, Vector3 position, Quaternion rotation, TweenCallback onComplete)
        {
            transition?.Kill();

            transition = DOTween.Sequence()
                .Join(cam.transform.DOMove(position, transitionDuration).SetEase(transitionEase))
                .Join(cam.transform.DORotateQuaternion(rotation, transitionDuration).SetEase(transitionEase))
                .OnComplete(onComplete);
        }
    }
}
