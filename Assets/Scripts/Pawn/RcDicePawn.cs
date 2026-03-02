using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(RcDiceFaceController))]
[RequireComponent(typeof(RcDiceMovement))]
[RequireComponent(typeof(RcDiceTileInteractor))]
public class RcDicePawn : MonoBehaviour
{
    private RcDiceFaceController faceController;
    private RcDiceMovement movement;
    private RcDiceTileInteractor tileInteractor;

    private void Awake()
    {
        faceController = GetComponent<RcDiceFaceController>();
        movement = GetComponent<RcDiceMovement>();
        tileInteractor = GetComponent<RcDiceTileInteractor>();

        Vector2Int startPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        faceController.Initialize();
        movement.Initialize(startPos);
        tileInteractor.Initialize(this);

        movement.OnMoveStarted += OnMoveStarted;
        movement.OnMoveCompleted += OnMoveCompleted;

        RcGameEvents.Instance.Subscribe(RcGameEvent.GameLose, OnGameLose);
    }

    private void Start()
    {
        // 카메라 타겟 설정
        if (RcDiceCamera.Instance != null)
        {
            RcDiceCamera.Instance.SetTarget(transform);
        }

        // 시작 타일에 진입
        tileInteractor.OnEnterTile(movement.GetGridPos());
    }

    private void OnDestroy()
    {
        if (movement == null) return;

        movement.OnMoveStarted -= OnMoveStarted;
        movement.OnMoveCompleted -= OnMoveCompleted;
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.GameLose, OnGameLose);
    }

    public void Move(Vector2Int direction)
    {
        if (movement.IsMoving())
            return;

        if (RcGameRuleManager.Instance.IsGameOver)
            return;

        Vector2Int targetPos = movement.GetGridPos() + direction;

        if (!IsValidMove(targetPos))
            return;

        movement.Move(direction);
        faceController.RotateFaces(direction);
    }

    public void Teleport(Vector2Int targetPos, float duration = 0.5f, System.Action onComplete = null)
    {
        movement.Teleport(targetPos, onComplete);
    }

    /// <summary>레벨 로드 시 GameBootstrap에서 호출. 프리팹 기본값 대신 레벨 지정 면 색을 적용한다.</summary>
    public void InitializeFaces(RcColorSO[] faces)
    {
        faceController.Initialize(faces);
    }

    public RcColorSO GetBottomColor()
    {
        return faceController.GetBottomColor();
    }

    public Vector2Int GetGridPos()
    {
        return movement.GetGridPos();
    }

    public RcColorSO GetFaceColor(int faceIndex)
    {
        return faceController.GetFaceColor(faceIndex);
    }

    private bool IsValidMove(Vector2Int targetPos)
    {
        RcTileData targetTile = RcLevelManager.Instance.GetRuntimeTile(targetPos);

        if (targetTile == null || targetTile.IsEmpty)
            return false;

        if (!targetTile.bCanEnter)
            return false;

        if (!tileInteractor.CanEnterTile(targetPos))
            return false;

        return true;
    }

    private void OnGameLose()
    {
        faceController.FadeToGray();
    }

    public void FlashEmission(System.Action onComplete = null)
    {
        faceController.FlashEmission(onComplete);
    }

    private void OnMoveStarted(Vector2Int fromPos)
    {
        tileInteractor.OnExitTile(fromPos);
        RcGameEvents.Instance.Publish(RcGameEvent.MoveStarted, fromPos);
    }

    private void OnMoveCompleted(Vector2Int toPos)
    {
        tileInteractor.OnEnterTile(toPos);
        RcGameEvents.Instance.Publish(RcGameEvent.MoveCompleted, toPos);
    }
}
