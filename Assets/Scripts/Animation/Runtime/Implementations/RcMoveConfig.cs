using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RcMoveConfig : RcTweenConfig
{
    [Header("Move")]
    public Vector3 targetPosition = Vector3.zero;
    public bool useLocalPosition = true;
    public bool fromCurrentPosition = true;
    public Vector3 startPosition = Vector3.zero;
    public bool snapping = false;

    [Header("Restore")]
    public bool restoreToOriginal = false;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        var resolvedTarget = restoreToOriginal
            ? (useLocalPosition ? t.localPosition : t.position)
            : targetPosition;

        if (!fromCurrentPosition)
        {
            if (useLocalPosition)
                t.localPosition = startPosition;
            else
                t.position = startPosition;
        }

        var tween = useLocalPosition
            ? t.DOLocalMove(resolvedTarget, duration, snapping)
            : t.DOMove(resolvedTarget, duration, snapping);

        return tween.SetEase(ease).SetDelay(delay);
    }
}
