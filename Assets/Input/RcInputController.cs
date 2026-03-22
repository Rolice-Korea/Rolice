using UnityEngine;


public class RcInputController : MonoBehaviour
{
    [SerializeField] private RcDicePawn pawn;
    [SerializeField] private float minSwipeDistance = 50f;

    public static RcInputController Instance { get; private set; }

    private RcInputManager inputManager;
    private bool inputBlocked;

    private void OnEnable()
    {
        Instance = this;
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
        if (Instance == this) Instance = null;
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

    public void TriggerMove(Vector2Int dir) => inputManager?.TriggerMove(dir);
    public void TriggerRotate(int dir) => inputManager?.TriggerRotate(dir);
}
