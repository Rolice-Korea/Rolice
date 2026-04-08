using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RcInputManager
{
    public event Action<Vector2Int> OnMoveInput;
    public event Action<int> OnCameraRotateInput;
    public event Action<int> OnStartCameraRotate;
    public event Action       OnStopCameraRotate;

    private InputAction moveUpAction;
    private InputAction moveDownAction;
    private InputAction moveLeftAction;
    private InputAction moveRightAction;

    private InputAction cameraLeftAction;
    private InputAction cameraRightAction;

    public RcInputManager(float minSwipeDistance = 50f)
    {
        BuildActions();
    }

    public void Init()
    {
        moveUpAction.started    += OnMoveUpStarted;
        moveDownAction.started  += OnMoveDownStarted;
        moveLeftAction.started  += OnMoveLeftStarted;
        moveRightAction.started += OnMoveRightStarted;

        cameraLeftAction.started   += OnCameraLeftStarted;
        cameraLeftAction.canceled  += OnCameraLeftCanceled;
        cameraRightAction.started  += OnCameraRightStarted;
        cameraRightAction.canceled += OnCameraRightCanceled;

        moveUpAction.Enable();
        moveDownAction.Enable();
        moveLeftAction.Enable();
        moveRightAction.Enable();
        cameraLeftAction.Enable();
        cameraRightAction.Enable();
    }

    public void End()
    {
        moveUpAction.started    -= OnMoveUpStarted;
        moveDownAction.started  -= OnMoveDownStarted;
        moveLeftAction.started  -= OnMoveLeftStarted;
        moveRightAction.started -= OnMoveRightStarted;

        cameraLeftAction.started   -= OnCameraLeftStarted;
        cameraLeftAction.canceled  -= OnCameraLeftCanceled;
        cameraRightAction.started  -= OnCameraRightStarted;
        cameraRightAction.canceled -= OnCameraRightCanceled;

        moveUpAction.Dispose();
        moveDownAction.Dispose();
        moveLeftAction.Dispose();
        moveRightAction.Dispose();
        cameraLeftAction.Dispose();
        cameraRightAction.Dispose();
    }

    private void BuildActions()
    {
        moveUpAction    = BuildButton("MoveUp",    "<Keyboard>/w", "<Keyboard>/upArrow");
        moveDownAction  = BuildButton("MoveDown",  "<Keyboard>/s", "<Keyboard>/downArrow");
        moveLeftAction  = BuildButton("MoveLeft",  "<Keyboard>/a", "<Keyboard>/leftArrow");
        moveRightAction = BuildButton("MoveRight", "<Keyboard>/d", "<Keyboard>/rightArrow");

        cameraLeftAction  = BuildButton("CameraLeft",  "<Keyboard>/q");
        cameraRightAction = BuildButton("CameraRight", "<Keyboard>/e");
    }

    private void OnMoveUpStarted(InputAction.CallbackContext _)    => OnMoveInput?.Invoke(Vector2Int.up);
    private void OnMoveDownStarted(InputAction.CallbackContext _)  => OnMoveInput?.Invoke(Vector2Int.down);
    private void OnMoveLeftStarted(InputAction.CallbackContext _)  => OnMoveInput?.Invoke(Vector2Int.left);
    private void OnMoveRightStarted(InputAction.CallbackContext _) => OnMoveInput?.Invoke(Vector2Int.right);

    private void OnCameraLeftStarted(InputAction.CallbackContext _)   => OnStartCameraRotate?.Invoke(1);
    private void OnCameraLeftCanceled(InputAction.CallbackContext _)  => OnStopCameraRotate?.Invoke();
    private void OnCameraRightStarted(InputAction.CallbackContext _)  => OnStartCameraRotate?.Invoke(-1);
    private void OnCameraRightCanceled(InputAction.CallbackContext _) => OnStopCameraRotate?.Invoke();

    public void TriggerMove(Vector2Int dir) => OnMoveInput?.Invoke(dir);
    public void TriggerRotate(int dir) => OnCameraRotateInput?.Invoke(dir);

    private static InputAction BuildButton(string name, params string[] paths)
    {
        var action = new InputAction(name, InputActionType.Button);
        foreach (var path in paths)
            action.AddBinding(path);
        return action;
    }
}
