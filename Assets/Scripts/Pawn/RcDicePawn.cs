using UnityEngine;

[RequireComponent(typeof(RcDiceFaceController))]
[RequireComponent(typeof(RcDiceMovement))]
[RequireComponent(typeof(RcDiceTileInteractor))]
public class RcDicePawn : MonoBehaviour
{
    // === 컴포넌트 참조 ===
    private RcDiceFaceController faceController;
    private RcDiceMovement movement;
    private RcDiceTileInteractor tileInteractor;
    
    private void Awake()
    {
        // 컴포넌트 가져오기
        faceController = GetComponent<RcDiceFaceController>();
        movement = GetComponent<RcDiceMovement>();
        tileInteractor = GetComponent<RcDiceTileInteractor>();

        // 초기 위치 설정
        Vector2Int startPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // 각 컴포넌트 초기화
        faceController.Initialize();
        movement.Initialize(startPos);
        tileInteractor.Initialize(this);

        // 이동 이벤트 구독
        movement.OnMoveStarted += OnMoveStarted;
        movement.OnMoveCompleted += OnMoveCompleted;
    }

    private void Start()
    {
        tileInteractor.OnEnterTile(movement.GetGridPos());
    }

    private void OnDestroy()
    {
        if (movement == null) return;
        
        movement.OnMoveStarted -= OnMoveStarted;
        movement.OnMoveCompleted -= OnMoveCompleted;
    }

    public void Move(Vector2Int direction)
    {
        if (movement.IsMoving()) return;

        Vector2Int targetPos = movement.GetGridPos() + direction;
        
        // 이동 가능 여부 확인 (기본 검증 + 타일 행동 검증)
        if (!IsValidMove(targetPos))
        {
            Debug.Log($"[DicePawn] 이동 불가: {targetPos}");
            return;
        }

        // 이동 실행 (애니메이션 + 면 회전)
        movement.Move(direction);
        faceController.RotateFaces(direction);
    }

    public void Teleport(Vector2Int targetPos, float duration = 0.5f, System.Action onComplete = null)
    {
        movement.Teleport(targetPos, onComplete);
    }

    public ColorType GetBottomColor()
    {
        return faceController.GetBottomColor();
    }

    public Vector2Int GetGridPos()
    {
        return movement.GetGridPos();
    }

    public ColorType GetFaceColor(int faceIndex)
    {
        return faceController.GetFaceColor(faceIndex);
    }

    private bool IsValidMove(Vector2Int targetPos)
    {
        RcTileData targetTile = RcLevelManager.Instance.GetRuntimeTile(targetPos);
        
        // 타일이 없으면 불가
        if (targetTile == null || string.IsNullOrEmpty(targetTile.TileID))
            return false;
        
        // 진입 불가 타일
        if (!targetTile.bCanEnter)
            return false;
        
        // 타일 행동의 CanEnter 체크
        if (!tileInteractor.CanEnterTile(targetPos))
            return false;
        
        return true;
    }

    private void OnMoveStarted(Vector2Int fromPos)
    {
        // 현재 타일에서 나가기
        tileInteractor.OnExitTile(fromPos);
    }

    private void OnMoveCompleted(Vector2Int toPos)
    {
        // 새 타일에 진입
        tileInteractor.OnEnterTile(toPos);
    }
}