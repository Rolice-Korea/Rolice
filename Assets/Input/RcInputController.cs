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
        if (isProcessing)
            return;

        if (RcGameRuleManager.Instance != null && RcGameRuleManager.Instance.IsGameOver)
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

        Vector2Int direction;

        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            direction = swipe.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            direction = swipe.y > 0 ? new Vector2Int(0, 1) : new Vector2Int(0, -1);

        if (pawn != null)
            pawn.Move(direction);
    }

    void HandleKeyboardInput()
    {
        if (pawn == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            pawn.Move(new Vector2Int(0, 1));
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            pawn.Move(new Vector2Int(0, -1));
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            pawn.Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            pawn.Move(Vector2Int.right);
    }

    public void SetProcessing(bool processing)
    {
        isProcessing = processing;
    }
}
