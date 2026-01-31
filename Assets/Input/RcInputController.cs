using UnityEngine;

public class RcInputController : MonoBehaviour
{
    [SerializeField] private RcDicePawn pawn;
    [SerializeField] private float minSwipeDistance = 50f;
    
    private Vector2 startPos;
    private bool isProcessing = false; // 애니메이션 중 입력 방지
    
    void Update()
    {
        // 입력 처리 중이거나 게임이 끝났으면 입력 무시
        if (isProcessing) 
            return;
        
        if (RcGameRuleManager.Instance != null && RcGameRuleManager.Instance.IsGameOver)
            return;
        
        // 터치/마우스 시작
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }
        
        // 터치/마우스 끝 (스와이프 감지)
        if (Input.GetMouseButtonUp(0))
        {
            DetectSwipe();
        }
        
        // 키보드 입력 (디버그/테스트용)
        HandleKeyboardInput();
    }
    
    void DetectSwipe()
    {
        Vector2 endPos = Input.mousePosition;
        Vector2 swipe = endPos - startPos;
        
        // 최소 거리 체크
        if (swipe.magnitude < minSwipeDistance)
            return;
        
        // 상하좌우 판정
        Vector2Int direction;
        
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // 좌우 스와이프
            direction = swipe.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            // 상하 스와이프
            direction = swipe.y > 0 ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        }
        
        // Pawn에게 이동 명령
        if (pawn != null)
        {
            pawn.Move(direction);
        }
        else
        {
            Debug.LogWarning("[InputController] Pawn이 할당되지 않았습니다!");
        }
    }

    void HandleKeyboardInput()
    {
        if (pawn == null) return;
        
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            pawn.Move(new Vector2Int(0, 1));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            pawn.Move(new Vector2Int(0, -1));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pawn.Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pawn.Move(Vector2Int.right);
        }
    }
    
    public void SetProcessing(bool processing)
    {
        isProcessing = processing;
    }
}