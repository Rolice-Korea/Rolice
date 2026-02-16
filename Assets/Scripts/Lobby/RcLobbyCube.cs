using UnityEngine;

public class RcLobbyCube : MonoBehaviour
{
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

    private Camera mainCamera;
    private Vector2 lastPointerPos;
    private Vector2 dragVelocity;
    private bool isDragging;
    private bool isFloatPaused;

    public bool IsDragging => isDragging;
    public bool HasInertia => dragVelocity.magnitude > minInertiaSpeed;

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
}
