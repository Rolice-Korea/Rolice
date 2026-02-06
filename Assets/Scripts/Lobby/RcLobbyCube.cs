using UnityEngine;

public class RcLobbyCube : MonoBehaviour
{
    #region Serialized Fields

    [Header("자동 회전")]
    [SerializeField] private float _autoRotateSpeed = 15f;
    [SerializeField] private Vector3 _autoRotateAxis = Vector3.up;

    [Header("드래그 회전")]
    [SerializeField] private float _dragSensitivity = 0.3f;
    [SerializeField] private float _inertiaDecaySpeed = 2f;
    [SerializeField] private float _minInertiaSpeed = 0.1f;

    [Header("플로팅 연동")]
    [SerializeField] private RcTweenAnimator _floatAnimator;
    [SerializeField] private bool _pauseFloatOnDrag = true;

    #endregion

    #region Private Fields

    private Camera _mainCamera;
    private Vector2 _lastPointerPos;
    private Vector2 _dragVelocity;
    private bool _isDragging;
    private bool _isFloatPaused;

    #endregion

    #region Properties

    public bool IsDragging => _isDragging;
    public bool HasInertia => _dragVelocity.magnitude > _minInertiaSpeed;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInput();
        UpdateRotation();
    }

    #endregion

    #region Input

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
            BeginDrag();
        else if (Input.GetMouseButtonUp(0))
            EndDrag();

        if (_isDragging)
            UpdateDrag();
    }

    private void BeginDrag()
    {
        _isDragging = true;
        _lastPointerPos = Input.mousePosition;
        _dragVelocity = Vector2.zero;
        SetFloatPaused(true);
    }

    private void EndDrag()
    {
        _isDragging = false;
    }

    private void UpdateDrag()
    {
        Vector2 currentPos = Input.mousePosition;
        Vector2 delta = currentPos - _lastPointerPos;

        _dragVelocity = delta * _dragSensitivity;
        _lastPointerPos = currentPos;
    }

    #endregion

    #region Rotation

    private void UpdateRotation()
    {
        if (_isDragging)
        {
            RotateByCamera(_dragVelocity);
            return;
        }

        if (HasInertia)
        {
            RotateByCamera(_dragVelocity);
            _dragVelocity = Vector2.Lerp(_dragVelocity, Vector2.zero, _inertiaDecaySpeed * Time.deltaTime);
        }
        else
        {
            _dragVelocity = Vector2.zero;
            SetFloatPaused(false);
            ApplyAutoRotation();
        }
    }

    private void RotateByCamera(Vector2 velocity)
    {
        if (_mainCamera == null) return;

        var camRight = _mainCamera.transform.right;
        var camUp = _mainCamera.transform.up;

        transform.Rotate(camUp, -velocity.x, Space.World);
        transform.Rotate(camRight, velocity.y, Space.World);
    }

    private void ApplyAutoRotation()
    {
        transform.Rotate(_autoRotateAxis * _autoRotateSpeed * Time.deltaTime, Space.World);
    }

    #endregion

    #region Float Animation

    private void SetFloatPaused(bool paused)
    {
        if (_floatAnimator == null || !_pauseFloatOnDrag) return;
        if (_isFloatPaused == paused) return;

        if (paused)
            _floatAnimator.Pause();
        else
            _floatAnimator.Resume();

        _isFloatPaused = paused;
    }

    #endregion
}
