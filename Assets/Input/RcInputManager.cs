using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RcInputManager
{
    public event Action<Vector2Int> OnMoveInput;
    public event Action<int> OnCameraRotateInput;

    private readonly float minSwipeDistance;

    private InputAction moveUpAction;
    private InputAction moveDownAction;
    private InputAction moveLeftAction;
    private InputAction moveRightAction;

    private InputAction pointerPressAction;
    private InputAction pointerPositionAction;

    private InputAction cameraLeftAction;
    private InputAction cameraRightAction;

    private Vector2 pointerStartPos;

    public RcInputManager(float minSwipeDistance = 50f)
    {
        this.minSwipeDistance = minSwipeDistance;
        BuildActions();
    }

    public void Init()
    {
        moveUpAction.started    += OnMoveUpStarted;
        moveDownAction.started  += OnMoveDownStarted;
        moveLeftAction.started  += OnMoveLeftStarted;
        moveRightAction.started += OnMoveRightStarted;

        pointerPressAction.started  += OnPointerPressStarted;
        pointerPressAction.canceled += OnPointerPressCanceled;

        cameraLeftAction.started  += OnCameraLeftStarted;
        cameraRightAction.started += OnCameraRightStarted;

        moveUpAction.Enable();
        moveDownAction.Enable();
        moveLeftAction.Enable();
        moveRightAction.Enable();
        pointerPressAction.Enable();
        pointerPositionAction.Enable();
        cameraLeftAction.Enable();
        cameraRightAction.Enable();
    }

    public void End()
    {
        moveUpAction.started    -= OnMoveUpStarted;
        moveDownAction.started  -= OnMoveDownStarted;
        moveLeftAction.started  -= OnMoveLeftStarted;
        moveRightAction.started -= OnMoveRightStarted;

        pointerPressAction.started  -= OnPointerPressStarted;
        pointerPressAction.canceled -= OnPointerPressCanceled;

        cameraLeftAction.started  -= OnCameraLeftStarted;
        cameraRightAction.started -= OnCameraRightStarted;

        moveUpAction.Dispose();
        moveDownAction.Dispose();
        moveLeftAction.Dispose();
        moveRightAction.Dispose();
        pointerPressAction.Dispose();
        pointerPositionAction.Dispose();
        cameraLeftAction.Dispose();
        cameraRightAction.Dispose();
    }

    private void BuildActions()
    {
        moveUpAction    = BuildButton("MoveUp",    "<Keyboard>/w", "<Keyboard>/upArrow");
        moveDownAction  = BuildButton("MoveDown",  "<Keyboard>/s", "<Keyboard>/downArrow");
        moveLeftAction  = BuildButton("MoveLeft",  "<Keyboard>/a", "<Keyboard>/leftArrow");
        moveRightAction = BuildButton("MoveRight", "<Keyboard>/d", "<Keyboard>/rightArrow");

        pointerPressAction = BuildButton("PointerPress",
            "<Mouse>/leftButton",
            "<Touchscreen>/primaryTouch/press");

        pointerPositionAction = new InputAction("PointerPosition", InputActionType.Value, expectedControlType: "Vector2");
        pointerPositionAction.AddBinding("<Mouse>/position");
        pointerPositionAction.AddBinding("<Touchscreen>/primaryTouch/position");

        cameraLeftAction  = BuildButton("CameraLeft",  "<Keyboard>/q");
        cameraRightAction = BuildButton("CameraRight", "<Keyboard>/e");
    }

    private void OnMoveUpStarted(InputAction.CallbackContext _)    => OnMoveInput?.Invoke(Vector2Int.up);
    private void OnMoveDownStarted(InputAction.CallbackContext _)  => OnMoveInput?.Invoke(Vector2Int.down);
    private void OnMoveLeftStarted(InputAction.CallbackContext _)  => OnMoveInput?.Invoke(Vector2Int.left);
    private void OnMoveRightStarted(InputAction.CallbackContext _) => OnMoveInput?.Invoke(Vector2Int.right);

    private void OnCameraLeftStarted(InputAction.CallbackContext _)  => OnCameraRotateInput?.Invoke(1);
    private void OnCameraRightStarted(InputAction.CallbackContext _) => OnCameraRotateInput?.Invoke(-1);

    private void OnPointerPressStarted(InputAction.CallbackContext _)
    {
        pointerStartPos = pointerPositionAction.ReadValue<Vector2>();
    }

    private void OnPointerPressCanceled(InputAction.CallbackContext _)
    {
        Vector2 delta = pointerPositionAction.ReadValue<Vector2>() - pointerStartPos;

        if (delta.magnitude < minSwipeDistance)
            return;

        Vector2Int dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? (delta.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (delta.y > 0 ? Vector2Int.up    : Vector2Int.down);

        OnMoveInput?.Invoke(dir);
    }

    private static InputAction BuildButton(string name, params string[] paths)
    {
        var action = new InputAction(name, InputActionType.Button);
        foreach (var path in paths)
            action.AddBinding(path);
        return action;
    }
}
