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

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        if (!fromCurrentPosition)
        {
            if (useLocalPosition)
                t.localPosition = startPosition;
            else
                t.position = startPosition;
        }

        var tween = useLocalPosition
            ? t.DOLocalMove(targetPosition, duration, snapping)
            : t.DOMove(targetPosition, duration, snapping);

        return tween.SetEase(ease).SetDelay(delay);
    }
}
