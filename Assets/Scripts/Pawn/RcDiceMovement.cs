using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 주사위의 이동, 애니메이션, 텔레포트를 담당하는 컴포넌트
/// - 이동 검증
/// - Roll 애니메이션
/// - Teleport 애니메이션
/// </summary>
public class RcDiceMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private RcInputController inputController;

    [Header("Settings")]
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float teleportDuration = 0.5f;

    private Vector2Int gridPos;
    private bool isMoving;

    // 이벤트
    public event Action<Vector2Int> OnMoveStarted;
    public event Action<Vector2Int> OnMoveCompleted;

    // 매니저 참조
    private RcLevelManager LevelManager => RcLevelManager.Instance;

    // ========================================
    // 초기화
    // ========================================
    
    public void Initialize(Vector2Int startPosition)
    {
        gridPos = startPosition;
        
        if (modelTransform == null)
        {
            modelTransform = transform.GetChild(0);
        }
        
        UpdatePosition(gridPos);
    }

    // ========================================
    // 외부 인터페이스
    // ========================================
    
    public Vector2Int GetGridPos() => gridPos;
    public bool IsMoving() => isMoving;

    /// <summary>
    /// 일반 이동 (Roll 애니메이션)
    /// </summary>
    public void Move(Vector2Int direction)
    {
        if (isMoving) return;

        Vector2Int targetPos = gridPos + direction;
        
        if (!IsValidMove(targetPos))
        {
            Debug.Log($"[DiceMovement] 이동 불가: {targetPos}");
            return;
        }

        StartCoroutine(RollCo(direction, targetPos));
    }

    /// <summary>
    /// 텔레포트로 즉시 위치를 이동합니다
    /// </summary>
    public void Teleport(Vector2Int targetPos, Action onComplete = null)
    {
        if (isMoving)
        {
            Debug.LogWarning("[DiceMovement] 이동 중에는 텔레포트할 수 없습니다");
            return;
        }
        
        if (!IsValidTeleportTarget(targetPos))
        {
            Debug.LogWarning($"[DiceMovement] 텔레포트 대상이 유효하지 않습니다: {targetPos}");
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(TeleportCo(targetPos, onComplete));
    }

    public void SetPosition(Vector2Int newPos)
    {
        gridPos = newPos;
        UpdatePosition(newPos);
    }

    private bool IsValidMove(Vector2Int targetPos)
    {
        RcTileData targetTile = LevelManager.GetRuntimeTile(targetPos);
        
        if (targetTile == null || string.IsNullOrEmpty(targetTile.TileID))
            return false;
        
        if (!targetTile.bCanEnter)
            return false;
        
        // 타일 행동의 CanEnter 체크는 RcDicePawn에서 처리
        // (타일 행동이 RcDicePawn을 필요로 하므로)
        
        return true;
    }

    private bool IsValidTeleportTarget(Vector2Int targetPos)
    {
        RcTileData targetTile = LevelManager.GetRuntimeTile(targetPos);
        return targetTile != null && !string.IsNullOrEmpty(targetTile.TileID);
    }

    private IEnumerator RollCo(Vector2Int dir, Vector2Int targetPos)
    {
        isMoving = true;
        inputController?.SetProcessing(true);

        OnMoveStarted?.Invoke(gridPos);

        // 데이터 업데이트
        gridPos = targetPos;

        // 시작/끝 상태
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, 1f, targetPos.y);
        Quaternion startRot = modelTransform.localRotation;

        // 방향별 회전축/이동벡터
        Vector3 axis, moveDir;
        if (dir.x != 0) // 좌우
        {
            axis = dir.x > 0 ? Vector3.back : Vector3.forward;
            moveDir = Vector3.right * dir.x;
        }
        else // 앞뒤
        {
            axis = dir.y < 0 ? Vector3.left : Vector3.right;
            moveDir = Vector3.forward * dir.y;
        }

        Quaternion endRot = Quaternion.AngleAxis(90f, axis) * startRot;
        Vector3 pivot = startPos + moveDir * 0.5f + Vector3.down * 0.5f;

        // 애니메이션 루프
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / rollDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 3); // Ease out cubic

            float angle = ease * 90f;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            
            transform.position = pivot + rot * (startPos - pivot);
            modelTransform.localRotation = Quaternion.Slerp(startRot, endRot, ease);

            yield return null;
        }

        // 최종 위치
        transform.position = endPos;
        modelTransform.localRotation = endRot;

        isMoving = false;
        inputController?.SetProcessing(false);
        
        OnMoveCompleted?.Invoke(targetPos);
    }

    private IEnumerator TeleportCo(Vector2Int targetPos, Action onComplete)
    {
        isMoving = true;
        inputController?.SetProcessing(true);

        OnMoveStarted?.Invoke(gridPos);

        // 시작 위치
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, 1f, targetPos.y);

        float halfDuration = teleportDuration * 0.5f;
        
        // Phase 1: 페이드 아웃
        float t = 0f;
        Vector3 fadeOutPos = startPos + Vector3.up * 0.5f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);
            
            transform.position = Vector3.Lerp(startPos, fadeOutPos, ease);
            
            yield return null;
        }

        // 위치 이동
        SetPosition(targetPos);
        transform.position = endPos + Vector3.up * 0.5f;

        // Phase 2: 페이드 인
        t = 0f;
        Vector3 fadeInStart = endPos + Vector3.up * 0.5f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);
            
            transform.position = Vector3.Lerp(fadeInStart, endPos, ease);
            
            yield return null;
        }

        // 최종 위치
        transform.position = endPos;

        isMoving = false;
        inputController?.SetProcessing(false);
        
        onComplete?.Invoke();
    }
    
    private void UpdatePosition(Vector2Int pos)
    {
        transform.position = new Vector3(pos.x, 1f, pos.y);
    }
}
