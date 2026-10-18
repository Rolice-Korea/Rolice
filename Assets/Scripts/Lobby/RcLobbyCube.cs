using DG.Tweening;
using Rolice;
using Rolice.System;
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

    [Header("프리뷰 스킨")]
    [Tooltip("면 렌더러(6슬롯). 비어 있으면 자식에서 자동 탐색")]
    [SerializeField] private MeshRenderer faceRenderer;
    [Tooltip("테두리 렌더러(1슬롯). 비어 있으면 자식에서 자동 탐색")]
    [SerializeField] private MeshRenderer edgeRenderer;
    [Tooltip("면 렌더러의 머티리얼 슬롯 인덱스별 색상. 로비 큐브는 구르지 않으므로 슬롯-색 고정 매핑으로 충분")]
    [SerializeField] private RcColorType[] slotColors =
    {
        RcColorType.Cyan, RcColorType.Green, RcColorType.Grey,
        RcColorType.Magenta, RcColorType.Yellow, RcColorType.White
    };

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
        ResolveRenderers();
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

    /// <summary>
    /// 인스펙터 미할당 대비 폴백. 슬롯 수로 구분한다(면 = slotColors 이상, 테두리 = 1).
    /// </summary>
    private void ResolveRenderers()
    {
        if (faceRenderer != null && edgeRenderer != null) return;

        int faceSlotCount = slotColors != null ? slotColors.Length : 0;
        foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
        {
            int count = r.sharedMaterials.Length;
            if (faceRenderer == null && faceSlotCount > 0 && count >= faceSlotCount) faceRenderer = r;
            else if (edgeRenderer == null && count == 1) edgeRenderer = r;
        }

        if (faceRenderer == null) Debug.LogWarning("[RcLobbyCube] faceRenderer를 찾지 못함. 스킨 프리뷰 비활성");
        if (edgeRenderer == null) Debug.LogWarning("[RcLobbyCube] edgeRenderer를 찾지 못함. 테두리 프리뷰 비활성");
    }

    private void Start()
    {
        mainCamera = Camera.main;

        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        // 씬에 박힌 정적 머티리얼 대신 저장된 스킨으로 시작
        RestoreActiveSkin();
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

    // ─── Preview Skin ───────────────────────────────────────────────────

    /// <summary>저장된(장착 중인) 스킨으로 프리뷰를 되돌린다.</summary>
    public void RestoreActiveSkin()
    {
        PreviewFaceSkin(RcSkinSystem.ActiveFaceSkinType);
        PreviewEdgeSkin(RcSkinSystem.ActiveEdgeSkinType);
    }

    /// <summary>
    /// 면 스킨을 즉시 적용(저장하지 않음).
    /// 스킨에 해당 색이 없으면 Default 스킨 머티리얼로 폴백한다.
    /// </summary>
    public void PreviewFaceSkin(RcFaceSkinType skinType)
    {
        var registry = RcDataTableManager.FaceSkinRegistry;
        if (registry == null) return;

        PreviewFaceSkin(registry.GetFaceData(skinType));
    }

    /// <summary>
    /// 스킨 테이블을 직접 지정해 적용. 상점처럼 ItemId로 테이블을 얻는 경로용
    /// (테이블 헤더의 SkinType 값에 의존하지 않기 위함).
    /// </summary>
    public void PreviewFaceSkin(RcFaceSkinDataTable skin)
    {
        if (faceRenderer == null || slotColors == null) return;

        var registry = RcDataTableManager.FaceSkinRegistry;
        if (registry == null) return;

        var fallback = registry.GetFaceData(RcFaceSkinType.Default);

        // sharedMaterials: 에셋 머티리얼을 교체만 하므로 인스턴스 생성(materials) 불필요
        var mats = faceRenderer.sharedMaterials;
        int count = Mathf.Min(mats.Length, slotColors.Length);
        for (int slot = 0; slot < count; slot++)
        {
            var color = slotColors[slot];
            if (color == RcColorType.None) continue;

            if (TryGetFaceMaterial(skin, color, out var mat) || TryGetFaceMaterial(fallback, color, out mat))
                mats[slot] = mat;
        }
        faceRenderer.sharedMaterials = mats;
    }

    /// <summary>
    /// 테두리 스킨을 즉시 적용(저장하지 않음).
    /// 해당 행/머티리얼이 없으면 Default로 폴백, 그것도 없으면 현재 상태 유지.
    /// </summary>
    public void PreviewEdgeSkin(RcEdgeSkinType skinType)
    {
        if (edgeRenderer == null) return;

        var table = RcDataTableManager.EdgeSkinDataTable;
        if (table == null) return;

        var mat = GetEdgeMaterial(table, skinType);
        if (mat == null && skinType != RcEdgeSkinType.Default)
            mat = GetEdgeMaterial(table, RcEdgeSkinType.Default);

        if (mat != null)
            edgeRenderer.sharedMaterial = mat;
    }

    private static bool TryGetFaceMaterial(RcFaceSkinDataTable skin, RcColorType color, out Material mat)
    {
        mat = null;
        return skin != null && skin.TryGetFaceMaterial(color, out mat);
    }

    private static Material GetEdgeMaterial(RcEdgeSkinDataTable table, RcEdgeSkinType skinType)
        => table.TryGetEdgeData(skinType, out var row) ? row.EdgeMaterial : null;

    private void OnDestroy()
    {
        slideTween?.Kill();
    }
}

