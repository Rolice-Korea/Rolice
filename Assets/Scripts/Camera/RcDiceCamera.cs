using UnityEngine;

public class RcDiceCamera : MonoBehaviour
{
    private static RcDiceCamera _instance;
    public static RcDiceCamera Instance
    {
        get
        {
            if (_instance == null) _instance = Object.FindFirstObjectByType<RcDiceCamera>();
            return _instance;
        }
    }

    [Header("Target Tracking")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 12f;      // 주사위와의 거리 (가로 기준)
    [SerializeField] private float heightOffset = 0.5f; // 주사위의 중심점 (축)
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Hold Rotation")]
    [SerializeField] private float rotateSpeed = 90f;
    private int rotateDirection;

    // 가로 기준 기준 종횡비 (16:9)
    private const float ReferenceAspect = 16f / 9f;

    [Header("Angles")]
    [SerializeField] private float pitch = 35f;  // 내려다보는 각도
    [SerializeField] private float yaw = 45f;    // 목표 수평 각도
    private float currentYaw = 45f;

    public bool IsRotating => Mathf.Abs(Mathf.DeltaAngle(currentYaw, yaw)) > 0.1f;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        currentYaw = yaw;
    }

    // 인스펙터에서 값을 수정할 때 에디터 화면에 즉시 반영
    private void OnValidate()
    {
        currentYaw = yaw;
        if (target != null)
        {
            ApplyCameraTransform();
        }
    }

    private void Start()
    {
        if (target != null)
        {
            ApplyCameraTransform(); 
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 홀드 회전 처리
        if (rotateDirection != 0)
        {
            yaw += rotateDirection * rotateSpeed * Time.deltaTime;
        }

        // 1. 수평 각도(Yaw)만 부드럽게 보간
        currentYaw = Mathf.LerpAngle(currentYaw, yaw, Time.deltaTime * rotationSmoothSpeed);

        // 2. 매 프레임 현재 각도에 따른 위치/회전 즉시 갱신 (직선 보간 X)
        ApplyCameraTransform();
    }

    private void ApplyCameraTransform()
    {
        // 현재 각도로 회전값 생성
        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0);

        // 회전 축(Pivot) 위치 계산
        Vector3 pivotPos = target.position + Vector3.up * heightOffset;

        // 세로모드에서 종횡비 보정: 화면이 좁아질수록 더 멀리 물러남
        float currentAspect = (float)Screen.width / Screen.height;
        float aspectScale = Mathf.Max(1f, ReferenceAspect / currentAspect);
        float effectiveDistance = distance * aspectScale;

        Vector3 position = pivotPos - (rotation * Vector3.forward * effectiveDistance);

        transform.position = position;
        transform.rotation = rotation;
    }

    public void Rotate(int direction)
    {
        yaw += direction * 90f;
    }

    public void StartCameraRotate(int direction)
    {
        rotateDirection = direction;
    }

    public void StopCameraRotate()
    {
        rotateDirection = 0;
    }

    public Vector2Int GetAdjustedDirection(Vector2Int inputDir)
    {
        if (inputDir == Vector2Int.zero) return Vector2Int.zero;

        // 현재 카메라의 실제 각도로 입력 방향을 회전
        float rad = -currentYaw * Mathf.Deg2Rad;

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        float rx = inputDir.x * cos - inputDir.y * sin;
        float ry = inputDir.x * sin + inputDir.y * cos;

        // 절대값이 큰 축을 우선하여 항상 4방향(상하좌우) 중 하나만 반환
        if (Mathf.Abs(rx) >= Mathf.Abs(ry))
            return new Vector2Int(rx > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, ry > 0 ? 1 : -1);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
