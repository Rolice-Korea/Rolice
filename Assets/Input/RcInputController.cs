using UnityEngine;

public class RcInputController : MonoBehaviour
{
    [SerializeField] private RcDicePawn pawn;
    [SerializeField] private float minSwipeDistance = 50f;
    
    private Vector2 startPos;
    private bool isProcessing = false; // 애니메이션 중 입력 방지
    
    void Update()
    {
        if (isProcessing)
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
        pawn.Move(direction);
    }
    
    // Pawn의 애니메이션 시작/끝 시 호출
    public void SetProcessing(bool processing)
    {
        isProcessing = processing;
    }
}