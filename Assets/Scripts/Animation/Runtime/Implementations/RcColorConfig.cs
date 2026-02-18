using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RcColorConfig : RcTweenConfig
{
    [Header("Color")]
    public Color targetColor = Color.white;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        Tween tween = null;

        if (t.TryGetComponent<TextMeshProUGUI>(out var textMesh))
        {
            tween = textMesh.DOColor(targetColor, duration);
        }
        else if (t.TryGetComponent<Image>(out var image))
        {
            tween = image.DOColor(targetColor, duration);
        }
        else if (t.TryGetComponent<SpriteRenderer>(out var sprite))
        {
            tween = sprite.DOColor(targetColor, duration);
        }

        return tween?.SetEase(ease).SetDelay(delay);
    }
}

