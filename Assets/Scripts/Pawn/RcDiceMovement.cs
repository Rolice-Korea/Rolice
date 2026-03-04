using System;
using System.Collections;
using UnityEngine;

public class RcDiceMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private RcInputController inputController;

    [Header("Settings")]
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float teleportDuration = 0.5f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Lean (Blocked Move)")]
    [SerializeField] private float leanAngle = 22f;
    [SerializeField] private float leanDuration = 0.2f;

    private Vector2Int gridPos;
    private bool isMoving;

    public event Action<Vector2Int> OnMoveStarted;
    public event Action<Vector2Int> OnMoveCompleted;
    public event Action<Vector2Int, float> OnRollStarted;

    private RcLevelManager LevelManager => RcLevelManager.Instance;

    public void Initialize(Vector2Int startPosition)
    {
        gridPos = startPosition;

        if (modelTransform == null)
            modelTransform = transform.GetChild(0);

        UpdatePosition(gridPos);
    }

    public Vector2Int GetGridPos() => gridPos;
    public bool IsMoving() => isMoving;

    public void Move(Vector2Int direction)
    {
        if (isMoving) return;

        Vector2Int targetPos = gridPos + direction;
        StartCoroutine(RollCo(direction, targetPos));
    }

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

    private bool IsValidTeleportTarget(Vector2Int targetPos)
    {
        RcTileData targetTile = LevelManager.GetRuntimeTile(targetPos);
        return targetTile != null && !targetTile.IsEmpty;
    }

    private IEnumerator RollCo(Vector2Int dir, Vector2Int targetPos)
    {
        isMoving = true;
        inputController?.SetProcessing(true);

        OnMoveStarted?.Invoke(gridPos);
        gridPos = targetPos;

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, heightOffset, targetPos.y);
        Quaternion startRot = modelTransform.localRotation;

        Vector3 axis, moveDir;
        if (dir.x != 0)
        {
            axis = dir.x > 0 ? Vector3.back : Vector3.forward;
            moveDir = Vector3.right * dir.x;
        }
        else
        {
            axis = dir.y < 0 ? Vector3.left : Vector3.right;
            moveDir = Vector3.forward * dir.y;
        }

        Quaternion endRot = Quaternion.AngleAxis(90f, axis) * startRot;
        Vector3 pivot = startPos + moveDir * 0.5f + Vector3.down * 0.5f;

        OnRollStarted?.Invoke(dir, rollDuration);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / rollDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 3);

            float angle = ease * 90f;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);

            transform.position = pivot + rot * (startPos - pivot);
            modelTransform.localRotation = Quaternion.Slerp(startRot, endRot, ease);

            yield return null;
        }

        transform.position = endPos;
        modelTransform.localRotation = endRot;

        isMoving = false;
        inputController?.SetProcessing(false);

        OnMoveCompleted?.Invoke(targetPos);
    }

    public void Lean(Vector2Int direction)
    {
        if (isMoving) return;
        StartCoroutine(LeanCo(direction));
    }

    private IEnumerator LeanCo(Vector2Int dir)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = modelTransform.localRotation;

        Vector3 axis, moveDir;
        if (dir.x != 0)
        {
            axis = dir.x > 0 ? Vector3.back : Vector3.forward;
            moveDir = Vector3.right * dir.x;
        }
        else
        {
            axis = dir.y < 0 ? Vector3.left : Vector3.right;
            moveDir = Vector3.forward * dir.y;
        }

        Vector3 pivot = startPos + moveDir * 0.5f + Vector3.down * 0.5f;
        Quaternion leanEndRot = Quaternion.AngleAxis(leanAngle, axis) * startRot;

        float halfDuration = leanDuration * 0.5f;

        // Phase 1: 막힌 방향으로 기울기
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);

            Quaternion rot = Quaternion.AngleAxis(ease * leanAngle, axis);
            transform.position = pivot + rot * (startPos - pivot);
            modelTransform.localRotation = Quaternion.Slerp(startRot, leanEndRot, ease);

            yield return null;
        }

        Vector3 leanedPos = transform.position;
        Quaternion leanedRot = modelTransform.localRotation;

        // Phase 2: 원위치 복귀
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);

            transform.position = Vector3.Lerp(leanedPos, startPos, ease);
            modelTransform.localRotation = Quaternion.Slerp(leanedRot, startRot, ease);

            yield return null;
        }

        transform.position = startPos;
        modelTransform.localRotation = startRot;

        isMoving = false;
    }

    private IEnumerator TeleportCo(Vector2Int targetPos, Action onComplete)
    {
        isMoving = true;
        inputController?.SetProcessing(true);

        OnMoveStarted?.Invoke(gridPos);

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, heightOffset, targetPos.y);

        float halfDuration = teleportDuration * 0.5f;

        // Phase 1: 상승
        float t = 0f;
        Vector3 fadeOutPos = startPos + Vector3.up * 0.5f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);
            transform.position = Vector3.Lerp(startPos, fadeOutPos, ease);
            yield return null;
        }

        SetPosition(targetPos);
        transform.position = endPos + Vector3.up * 0.5f;

        // Phase 2: 하강
        t = 0f;
        Vector3 fadeInStart = endPos + Vector3.up * 0.5f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / halfDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 2);
            transform.position = Vector3.Lerp(fadeInStart, endPos, ease);
            yield return null;
        }

        transform.position = endPos;

        isMoving = false;
        inputController?.SetProcessing(false);

        onComplete?.Invoke();
    }

    private void UpdatePosition(Vector2Int pos)
    {
        transform.position = new Vector3(pos.x, heightOffset, pos.y);
    }
}
