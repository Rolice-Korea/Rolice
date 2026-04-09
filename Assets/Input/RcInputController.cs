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
        inputManager.OnStartCameraRotate += OnStartCameraRotate;
        inputManager.OnStopCameraRotate  += OnStopCameraRotate;
        inputManager.Init();
    }

    private void OnDisable()
    {
        if (inputManager == null) return;

        inputManager.OnMoveInput -= OnMove;
        inputManager.OnStartCameraRotate -= OnStartCameraRotate;
        inputManager.OnStopCameraRotate  -= OnStopCameraRotate;
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

    private void OnStartCameraRotate(int direction)
    {
        RcDiceCamera.Instance?.StartCameraRotate(direction);
    }

    private void OnStopCameraRotate()
    {
        RcDiceCamera.Instance?.StopCameraRotate();
    }

    /// <summary>
    /// 외부에서 이미 카메라 보정이 완료된 방향으로 이동 (예: 스와이프)
    /// GetAdjustedDirection을 거치지 않음
    /// </summary>
    public void TriggerMove(Vector2Int dir)
    {
        if (inputBlocked || pawn == null) return;
        pawn.Move(dir);
    }
    public void TriggerRotate(int dir) => inputManager?.TriggerRotate(dir);

    public void StartCameraRotate(int dir) => RcDiceCamera.Instance?.StartCameraRotate(dir);
    public void StopCameraRotate()         => RcDiceCamera.Instance?.StopCameraRotate();
}
