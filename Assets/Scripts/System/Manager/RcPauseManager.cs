using DG.Tweening;
using Engine;
using UnityEngine;

public class RcPauseManager : RcSingletonMono<RcPauseManager>
{
    [SerializeField] private RcInputController inputController;

    [Header("Blur")]
    [SerializeField] private float blurInDuration = 0.3f;
    [SerializeField] private float blurOutDuration = 0.2f;
    [SerializeField] private float blurMaxAmount = 3f;

    private Tween blurTween;
    private bool isResumed = true;

    public void Pause()
    {
        if (!isResumed) return;
        isResumed = false;

        inputController?.SetProcessing(true);

        blurTween?.Kill();
        float current = 0f;
        blurTween = DOTween.To(
            () => current,
            v => { current = v; SetBlur(v); },
            blurMaxAmount,
            blurInDuration
        ).SetEase(Ease.OutQuad);
    }

    public void Resume()
    {
        if (isResumed) return;
        isResumed = true;

        blurTween?.Kill();
        float current = blurMaxAmount;
        blurTween = DOTween.To(
            () => current,
            v => { current = v; SetBlur(v); },
            0f,
            blurOutDuration
        ).SetEase(Ease.InQuad)
        .OnComplete(() =>
        {
            SetBlur(0f);
            if (inputController != null) inputController.SetProcessing(false);
        });
    }

    private void SetBlur(float amount)
    {
        if (RcPauseBlurFeature.Instance != null)
            RcPauseBlurFeature.Instance.BlurAmount = amount;
    }

    private void OnDestroy()
    {
        blurTween?.Kill();
        SetBlur(0f);
    }
}
