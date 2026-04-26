using DG.Tweening;
using UnityEngine;
using Rolice;
using Rolice.System;


public class RcDiceIconWidget : MonoBehaviour
{
    [SerializeField] private RcDicePawn dicePawn;
    [SerializeField] private Transform diceModelTransform; // DicePawn 하위 실제 모델 Transform
    [SerializeField] private Transform iconRoot;           // dice_edge: 회전 동기화 대상
    [SerializeField] private Renderer bodyRenderer;        // dice(Quad): 색상 표시 면
    [SerializeField] private Transform overlayCam;         // OverlayCam: 게임 카메라 rotation 동기화
    [SerializeField] private Vector2 viewportAnchor = new Vector2(0.1f, 0.1f); // 뷰포트 기준 위치
    [SerializeField] private float revealPulseMultiplier = 3f;  // 등장 시 이미션 배율
    [SerializeField] private float revealPulseDuration   = 0.35f;

    private Material bodyMaterialInstance;
    private Quaternion initialModelRot;  // 게임 시작 시 dice model의 초기 rotation
    private Quaternion initialIconRot;   // 게임 시작 시 iconRoot의 초기 rotation
    private Camera overlayCamComponent;
    private float widgetDepth;

    private static readonly int ShaderGlowColor = Shader.PropertyToID("_GlowColor");

    private void Start()
    {
        bodyMaterialInstance = bodyRenderer.material;
        overlayCamComponent = overlayCam.GetComponent<Camera>();
        widgetDepth = Vector3.Distance(overlayCam.position, transform.position);

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

        // 뷰포트 좌표 기반으로 위젯 위치 이동 (해상도 무관)
        if (overlayCamComponent != null)
            transform.position = overlayCamComponent.ViewportToWorldPoint(
                new Vector3(viewportAnchor.x, viewportAnchor.y, widgetDepth));

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
        PlayRevealPulse();
    }

    private void PlayRevealPulse()
    {
        Color baseColor = bodyMaterialInstance.GetColor(ShaderGlowColor);
        bodyMaterialInstance.SetColor(ShaderGlowColor, baseColor * revealPulseMultiplier);
        DOTween.To(
            () => bodyMaterialInstance.GetColor(ShaderGlowColor),
            c  => bodyMaterialInstance.SetColor(ShaderGlowColor, c),
            baseColor,
            revealPulseDuration
        ).SetEase(Ease.OutCubic);
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
        if (bottomColor == RcColorType.None) return;

        var table = RcDataTableManager.FaceDataTable;
        if (table == null) return;
        var skin = table.GetFaceData(RcSkinSystem.ActiveFaceSkinType);
        var material = skin?.GetFaceMaterial(bottomColor);
        if (material == null) return;

        Color glowColor = material.GetColor(ShaderGlowColor);
        bodyMaterialInstance.SetColor(ShaderGlowColor, glowColor);
    }
}
