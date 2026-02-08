using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RcFadeConfig : RcTweenConfig
{
    [Header("Fade")]
    [Range(0f, 1f)] public float targetAlpha = 0f;
    public bool fromCurrentAlpha = true;
    [Range(0f, 1f)] public float startAlpha = 1f;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        Tween tween = null;

        if (t.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            if (!fromCurrentAlpha) canvasGroup.alpha = startAlpha;
            tween = canvasGroup.DOFade(targetAlpha, duration);
        }
        else if (t.TryGetComponent<SpriteRenderer>(out var sprite))
        {
            if (!fromCurrentAlpha)
            {
                var c = sprite.color;
                c.a = startAlpha;
                sprite.color = c;
            }
            tween = sprite.DOFade(targetAlpha, duration);
        }
        else if (t.TryGetComponent<Image>(out var image))
        {
            if (!fromCurrentAlpha)
            {
                var c = image.color;
                c.a = startAlpha;
                image.color = c;
            }
            tween = image.DOFade(targetAlpha, duration);
        }

        return tween?.SetEase(ease).SetDelay(delay);
    }
}
