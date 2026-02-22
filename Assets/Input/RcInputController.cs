using UnityEngine;


public class RcInputController : MonoBehaviour
{
    [SerializeField] private RcDicePawn pawn;
    [SerializeField] private float minSwipeDistance = 50f;

    private RcInputManager inputManager;
    private bool inputBlocked;

    private void OnEnable()
    {
        inputManager = new RcInputManager(minSwipeDistance);
        inputManager.OnMoveInput += OnMove;
        inputManager.OnCameraRotateInput += OnCameraRotate;
        inputManager.Init();
    }

    private void OnDisable()
    {
        if (inputManager == null) return;

        inputManager.OnMoveInput -= OnMove;
        inputManager.OnCameraRotateInput -= OnCameraRotate;
        inputManager.End();
        inputManager = null;
    }

    public void SetProcessing(bool processing)
    {
        inputBlocked = processing;
    }

    private void OnMove(Vector2Int rawDir)
    {
        if (inputBlocked || pawn == null) return;

        var dir = RcDiceCamera.Instance != null
            ? RcDiceCamera.Instance.GetAdjustedDirection(rawDir)
            : rawDir;

        pawn.Move(dir);
    }

    private void OnCameraRotate(int direction)
    {
        RcDiceCamera.Instance?.Rotate(direction);
    }
}
