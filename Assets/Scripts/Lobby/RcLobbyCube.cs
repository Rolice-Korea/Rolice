using DG.Tweening;
using UnityEngine;

public class RcLobbyCube : MonoBehaviour
{
    private static RcLobbyCube _instance;
    public static RcLobbyCube Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<RcLobbyCube>();
            return _instance;
        }
    }

    [Header("자동 회전")]
    [SerializeField] private float autoRotateSpeed = 15f;
    [SerializeField] private Vector3 autoRotateAxis = Vector3.up;

    [Header("드래그 회전")]
    [SerializeField] private float dragSensitivity = 0.3f;
    [SerializeField] private float inertiaDecaySpeed = 2f;
    [SerializeField] private float minInertiaSpeed = 0.1f;

    [Header("플로팅 연동")]
    [SerializeField] private RcTweenAnimator floatAnimator;
    [SerializeField] private bool pauseFloatOnDrag = true;

    [Header("프리뷰 슬라이드")]
    [SerializeField] private Vector3 previewOffset = new(-4f, -2.5f, 0f);
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    private Camera mainCamera;
    private Vector2 lastPointerPos;
    private Vector2 dragVelocity;
    private bool isDragging;
    private bool isFloatPaused;

    private Vector3 originPosition;
    private Transform slideRoot; // 플로팅(localPosition)과 충돌 방지용 부모. SlideToPreview/Center가 이걸 이동시킴
    private Tween slideTween;    // 현재 재생 중인 슬라이드 애니메이션. 중복 재생 방지용

    public bool IsDragging => isDragging;
    public bool HasInertia => dragVelocity.magnitude > minInertiaSpeed;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        originPosition = transform.position;
        CreateSlideRoot();
    }

    /// <summary>
    /// 플로팅 애니메이션(localPosition)과 슬라이드가 충돌하지 않도록
    /// 런타임에 부모 오브젝트를 삽입한다. 슬라이드는 부모를 이동시킨다.
    /// </summary>
    private void CreateSlideRoot()
    {
        var go = new GameObject("LobbyCubeSlideRoot");
        go.transform.SetPositionAndRotation(transform.position, Quaternion.identity);

        var originalParent = transform.parent;
        go.transform.SetParent(originalParent, worldPositionStays: true);
        transform.SetParent(go.transform, worldPositionStays: true);

        slideRoot = go.transform;
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    private void Update()
    {
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
        if (!Physics.Raycast(ray, out var hit) || hit.transform != transform)
            return;

        isDragging = true;
        lastPointerPos = Input.mousePosition;
        dragVelocity = Vector2.zero;
        SetFloatPaused(true);
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    private void UpdateDrag()
    {
        Vector2 currentPos = Input.mousePosition;
        dragVelocity = (currentPos - lastPointerPos) * dragSensitivity;
        lastPointerPos = currentPos;
    }

    private void UpdateRotation()
    {
        if (isDragging)
        {
            RotateByCamera(dragVelocity);
            return;
        }

        if (HasInertia)
        {
            RotateByCamera(dragVelocity);
            dragVelocity = Vector2.Lerp(dragVelocity, Vector2.zero, inertiaDecaySpeed * Time.deltaTime);
        }
        else
        {
            dragVelocity = Vector2.zero;
            SetFloatPaused(false);
            ApplyAutoRotation();
        }
    }

    private void RotateByCamera(Vector2 velocity)
    {
        if (mainCamera == null) return;

        transform.Rotate(mainCamera.transform.up, -velocity.x, Space.World);
        transform.Rotate(mainCamera.transform.right, velocity.y, Space.World);
    }

    private void ApplyAutoRotation()
    {
        transform.Rotate(autoRotateAxis * autoRotateSpeed * Time.deltaTime, Space.World);
    }

    private void SetFloatPaused(bool paused)
    {
        if (floatAnimator == null || !pauseFloatOnDrag) return;
        if (isFloatPaused == paused) return;

        if (paused) floatAnimator.Pause();
        else floatAnimator.Resume();

        isFloatPaused = paused;
    }

    /// <summary>패널 열림 시 호출. 주사위를 프리뷰 위치로 이동.</summary>
    public void SlideToPreview()
    {
        slideTween?.Kill();
        slideTween = slideRoot.DOMove(originPosition + previewOffset, slideDuration)
            .SetEase(slideEase);
    }

    /// <summary>패널 닫힘 시 호출. 주사위를 원위치로 복귀.</summary>
    public void SlideToCenter()
    {
        slideTween?.Kill();
        slideTween = slideRoot.DOMove(originPosition, slideDuration)
            .SetEase(slideEase);
    }

    private void OnDestroy()
    {
        slideTween?.Kill();
    }
}

