using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RcScaleConfig : RcTweenConfig
{
    [Header("Scale")]
    public Vector3 targetScale = Vector3.one * 1.2f;
    public bool fromCurrentScale = true;
    public Vector3 startScale = Vector3.one;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        if (!fromCurrentScale)
            t.localScale = startScale;

        return t.DOScale(targetScale, duration).SetEase(ease).SetDelay(delay);
    }
}
