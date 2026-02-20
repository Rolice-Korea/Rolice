using System;
using UnityEngine;

public class RcInputController : MonoBehaviour
{
    [SerializeField] private RcDicePawn pawn;
    [SerializeField] private float minSwipeDistance = 50f;

    private Vector2 startPos;
    private bool isProcessing;

    void Update()
    {
        // 1. 이동 애니메이션 중이면 모든 입력 차단
        if (isProcessing) 
            return;

        // 2. 게임 오버 체크
        if (RcGameRuleManager.Instance != null && RcGameRuleManager.Instance.IsGameOver)
            return;

        // 3. 카메라 회전 입력 (Q, E) - 회전 중일 때도 추가 회전이 가능하도록 우선 처리
        HandleCameraRotation();

        // 4. 이동 입력 (WASD) - 카메라가 회전 중일 때는 이동 차단
        if (RcDiceCamera.Instance != null && RcDiceCamera.Instance.IsRotating)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            DetectSwipe();
        }

        HandleKeyboardInput();
    }

    void DetectSwipe()
    {
        Vector2 endPos = Input.mousePosition;
        Vector2 swipe = endPos - startPos;

        if (swipe.magnitude < minSwipeDistance)
            return;

        Vector2Int inputDir;
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            inputDir = swipe.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            inputDir = swipe.y > 0 ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        
        Vector2Int adjustedDir = inputDir;
        if (RcDiceCamera.Instance != null)
            adjustedDir = RcDiceCamera.Instance.GetAdjustedDirection(inputDir);

        if (pawn != null)
            pawn.Move(adjustedDir);
    }

    void HandleCameraRotation()
    {
        // Q/E 입력 감지
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (RcDiceCamera.Instance != null)
                RcDiceCamera.Instance.Rotate(-1);
            else
                Debug.LogWarning("[InputController] RcDiceCamera.Instance를 찾을 수 없습니다. 씬에 배치되어 있는지 확인하세요.");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (RcDiceCamera.Instance != null)
                RcDiceCamera.Instance.Rotate(1);
            else
                Debug.LogWarning("[InputController] RcDiceCamera.Instance를 찾을 수 없습니다. 씬에 배치되어 있는지 확인하세요.");
        }
    }

    void HandleKeyboardInput()
    {
        if (pawn == null) return;

        Vector2Int inputDir = Vector2Int.zero;
        string keyName = "";

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            inputDir = new Vector2Int(0, 1);
            keyName = Input.GetKeyDown(KeyCode.W) ? "W" : "UpArrow";
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            inputDir = new Vector2Int(0, -1);
            keyName = Input.GetKeyDown(KeyCode.S) ? "S" : "DownArrow";
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            inputDir = Vector2Int.left;
            keyName = Input.GetKeyDown(KeyCode.A) ? "A" : "LeftArrow";
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            inputDir = Vector2Int.right;
            keyName = Input.GetKeyDown(KeyCode.D) ? "D" : "RightArrow";
        }

        if (inputDir != Vector2Int.zero)
        {
            Debug.Log($"[Input] Key: {keyName}");

            Vector2Int adjustedDir = inputDir;
            if (RcDiceCamera.Instance != null)
                adjustedDir = RcDiceCamera.Instance.GetAdjustedDirection(inputDir);
            
            pawn.Move(adjustedDir);
        }
    }

    public void SetProcessing(bool processing)
    {
        isProcessing = processing;
    }
}
