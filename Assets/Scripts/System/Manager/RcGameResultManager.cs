using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Engine;
using Engine.UI;
using Rolice.Particle;
using Rolice.UI;
using UnityEngine;

public class RcGameResultManager : RcSingletonMono<RcGameResultManager>
{
    [Header("Victory Effect")]
    [SerializeField] private RcParticleEffect victoryEffectPrefab;
    [SerializeField] private float effectYOffset = 0.5f;

    [Header("Camera Zoom")]
    [SerializeField] private float zoomOutAmount = 0.5f;
    [SerializeField] private float zoomDuration  = 0.8f;

    private int   currentStageNumber;
    private float originalCameraSize;
    private Tween cameraZoomTween;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Initialize(int stageNumber)
    {
        currentStageNumber = stageNumber;

        var cam = Camera.main;
        if (cam != null)
            originalCameraSize = cam.orthographicSize;

        UnsubscribeEvents();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        ResetCamera();
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        RcGameEvents.Instance.Subscribe(RcGameEvent.GameWin, OnGameWin);
    }

    private void UnsubscribeEvents()
    {
        RcGameEvents.Instance.Unsubscribe(RcGameEvent.GameWin, OnGameWin);
    }

    private void OnGameWin()
    {
        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(0.3f);

        bool flashDone = false;
        var  dice      = FindAnyObjectByType<RcDicePawn>();

        if (dice != null)
            dice.FlashEmission(() => flashDone = true);
        else
            flashDone = true;

        bool effectDone = victoryEffectPrefab == null;

        if (victoryEffectPrefab != null && dice != null)
        {
            var pos    = dice.transform.position + Vector3.up * effectYOffset;
            var effect = Instantiate(victoryEffectPrefab, pos, Quaternion.identity);
            effect.OnCompleted += () =>
            {
                effectDone = true;
                Destroy(effect.gameObject);
            };
            effect.PlayAsync().Forget();
        }

        yield return new WaitUntil(() => flashDone && effectDone);

        ShowVictoryResult();
    }

    private void ShowVictoryResult()
    {
        int   moveCount   = RcGameRuleManager.Instance.CurrentTurn;
        float elapsedTime = RcGameRuleManager.Instance.ElapsedTime;

        if (currentStageNumber > 0)
            RcProgressManager.Instance.RecordStageClear(currentStageNumber, moveCount, elapsedTime);

        int stars = 0;
        if (currentStageNumber > 0)
        {
            var levelData = RcProgressManager.Instance.StageDatabase.GetStage(currentStageNumber);
            stars = levelData.StageInfo.CalculateStars(moveCount, elapsedTime);
        }

        bool hasNext = currentStageNumber > 0
            && currentStageNumber < RcProgressManager.Instance.TotalStageCount;

        ShowResultPanel(moveCount, elapsedTime, stars, hasNext);
    }

    private void PlayCameraZoomOut(System.Action onComplete = null)
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            onComplete?.Invoke();
            return;
        }

        cameraZoomTween?.Kill();
        cameraZoomTween = DOTween.To(
            () => cam.orthographicSize,
            x => cam.orthographicSize = x,
            originalCameraSize + zoomOutAmount,
            zoomDuration
        ).SetEase(Ease.OutSine)
         .OnComplete(() => onComplete?.Invoke());
    }

    private void ResetCamera()
    {
        cameraZoomTween?.Kill();
        var cam = Camera.main;
        if (cam != null && cam.orthographic)
            cam.orthographicSize = originalCameraSize;
    }

    private void ShowResultPanel(int moveCount, float elapsedTime, int starCount, bool hasNextStage)
    {
        var data = new RcGameResultData
        {
            IsVictory    = true,
            MoveCount    = moveCount,
            ElapsedTime  = elapsedTime,
            StarCount    = starCount,
            HasNextStage = hasNextStage
        };

        RcUIManager.Instance.Open<RcGameResultPanel, RcGameResultData>(data);
    }
}
