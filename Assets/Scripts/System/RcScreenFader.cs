using DG.Tweening;
using Engine;
using UnityEngine;
using UnityEngine.UI;

public class RcScreenFader : RcSingletonMono<RcScreenFader>
{
    private const int FaderSortingOrder = 9999;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Tween activeTween;

    private void Awake()
    {
        InitializeSingleton();
        if (Instance != this) return;

        CreateFaderCanvas();
    }

    private void OnDestroy()
    {
        activeTween?.Kill();
    }

    public Tween FadeOut(float duration = 0.5f)
    {
        return Fade(1f, duration);
    }

    public Tween FadeIn(float duration = 0.5f)
    {
        return Fade(0f, duration);
    }

    public void SetBlack()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void SetClear()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private Tween Fade(float targetAlpha, float duration)
    {
        activeTween?.Kill();
        canvasGroup.blocksRaycasts = true;

        activeTween = canvasGroup
            .DOFade(targetAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                if (targetAlpha <= 0f)
                    canvasGroup.blocksRaycasts = false;

                activeTween = null;
            });

        return activeTween;
    }

    private void CreateFaderCanvas()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = FaderSortingOrder;

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(transform, false);

        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
