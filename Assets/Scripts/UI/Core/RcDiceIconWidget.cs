using UnityEngine;

/// <summary>
/// 다이스 아이콘 위젯.
/// - iconRoot(dice_edge 프레임): 매 LateUpdate마다 실제 다이스 모델 rotation을 복사 → 완벽 동기화
/// - bodyRenderer(Quad 면): 굴리는 동안 숨기고 도착 후 바닥면 색으로 표시
/// 아이콘 오브젝트는 Overlay Camera 전용 레이어에 배치할 것.
/// </summary>
public class RcDiceIconWidget : MonoBehaviour
{
    [SerializeField] private RcDicePawn dicePawn;
    [SerializeField] private Transform diceModelTransform; // DicePawn 하위 실제 모델 Transform
    [SerializeField] private Transform iconRoot;           // dice_edge: 회전 동기화 대상
    [SerializeField] private Renderer bodyRenderer;        // dice(Quad): 색상 표시 면
    [SerializeField] private Transform overlayCam;         // OverlayCam: 게임 카메라 rotation 동기화

    private Material bodyMaterialInstance;
    private Quaternion initialModelRot;  // 게임 시작 시 dice model의 초기 rotation
    private Quaternion initialIconRot;   // 게임 시작 시 iconRoot의 초기 rotation

    private static readonly int ShaderGlowColor = Shader.PropertyToID("_GlowColor");

    private void Start()
    {
        bodyMaterialInstance = bodyRenderer.material;

        // FaceController 초기화 이후 rotation을 기준점으로 저장
        initialModelRot = diceModelTransform != null ? diceModelTransform.rotation : Quaternion.identity;
        initialIconRot  = iconRoot.rotation;

        UpdateBottomColor();

        dicePawn.OnRollStarted += OnRollStarted;
        RcGameEvents.Instance.Subscribe(RcGameEvent.MoveCompleted, OnMoveCompleted);
        RcGameEvents.Instance.Subscribe(RcGameEvent.GameLose, OnGameLose);
    }

    private void LateUpdate()
    {
        // OverlayCam을 Main Camera rotation에 동기화 (Q/E 카메라 회전 대응)
        if (overlayCam != null && Camera.main != null)
            overlayCam.rotation = Camera.main.transform.rotation;

        // 초기 rotation 대비 delta만 추출 → iconRoot 초기 rotation에 합성
        if (diceModelTransform == null) return;
        Quaternion delta = diceModelTransform.rotation * Quaternion.Inverse(initialModelRot);
        iconRoot.rotation = delta * initialIconRot;
    }

    private void OnDestroy()
    {
        if (dicePawn != null)
            dicePawn.OnRollStarted -= OnRollStarted;

        if (RcGameEvents.Instance != null)
        {
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.MoveCompleted, OnMoveCompleted);
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.GameLose, OnGameLose);
        }

        if (bodyMaterialInstance != null)
            Destroy(bodyMaterialInstance);
    }

    private void OnRollStarted(Vector2Int dir, float duration)
    {
        bodyRenderer.enabled = false;
    }

    private void OnMoveCompleted(Vector2Int pos)
    {
        UpdateBottomColor();
        bodyRenderer.enabled = true;
    }

    private void OnGameLose()
    {
        if (bodyMaterialInstance != null)
            bodyMaterialInstance.SetColor(ShaderGlowColor, Color.gray * 0.3f);
    }

    private void UpdateBottomColor()
    {
        if (bodyMaterialInstance == null) return;

        var bottomColor = dicePawn.GetBottomColor();
        if (bottomColor == null || bottomColor.DiceMaterial == null) return;

        Color glowColor = bottomColor.DiceMaterial.GetColor(ShaderGlowColor);
        bodyMaterialInstance.SetColor(ShaderGlowColor, glowColor);
    }
}
