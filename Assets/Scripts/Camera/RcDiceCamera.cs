using UnityEngine;

/// <summary>
/// 언리얼의 스프링 암처럼 작동하는 카메라.
/// 타겟을 중심으로 완벽하게 공전(Orbit)하며, 직선 보간 없이 각도에 따른 좌표를 즉시 계산합니다.
/// </summary>
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
    [SerializeField] private float distance = 12f;      // 주사위와의 거리
    [SerializeField] private float heightOffset = 0.5f; // 주사위의 중심점 (축)
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Angles")]
    [SerializeField] private float pitch = 35f;  // 내려다보는 각도
    [SerializeField] private float targetYaw = 45f; // 목표 수평 각도
    private float currentYaw = 45f;

    public bool IsRotating => Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw)) > 0.1f;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        currentYaw = targetYaw;
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

        // 1. 수평 각도(Yaw)만 부드럽게 보간
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * rotationSmoothSpeed);

        // 2. 매 프레임 현재 각도에 따른 위치/회전 즉시 갱신 (직선 보간 X)
        ApplyCameraTransform();
    }

    private void ApplyCameraTransform()
    {
        // 현재 각도로 회전값 생성
        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0);

        // 회전 축(Pivot) 위치 계산
        Vector3 pivotPos = target.position + Vector3.up * heightOffset;

        // 회전 방향의 반대(뒤쪽)로 거리만큼 떨어진 위치를 '직접' 계산
        // 이 방식은 직선으로 이동하지 않고 항상 pivotPos 주위를 원형으로 돕니다.
        Vector3 position = pivotPos - (rotation * Vector3.forward * distance);

        transform.position = position;
        transform.rotation = rotation;
    }

    public void Rotate(int direction)
    {
        targetYaw += direction * 90f;
    }

    public Vector2Int GetAdjustedDirection(Vector2Int inputDir)
    {
        if (inputDir == Vector2Int.zero) return Vector2Int.zero;

        // 쿼터뷰(45도) 기준 조작을 위해 입력 벡터를 카메라 회전에 맞춰 회전시킵니다.
        // 초기 45도 각도에서 W가 월드 Z축(0, 1)이 되도록 -45도 오프셋을 적용합니다.
        float angle = targetYaw + 45f;
        float rad = -angle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        // 2D 벡터 회전 공식 (Y축 기준 회전이므로 평면상의 회전과 동일)
        float rx = inputDir.x * cos - inputDir.y * sin;
        float ry = inputDir.x * sin + inputDir.y * cos;

        // 회전된 결과를 가장 가까운 정수 축 방향으로 반올림하여 반환합니다.
        return new Vector2Int(Mathf.RoundToInt(rx), Mathf.RoundToInt(ry));
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
