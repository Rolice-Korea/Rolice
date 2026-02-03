using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RcRotateConfig : RcTweenConfig
{
    [Header("Rotate")]
    public Vector3 targetRotation = new Vector3(0, 0, 360);
    public RotateMode rotateMode = RotateMode.Fast;
    public bool useLocalRotation = true;
    public bool fromCurrentRotation = true;
    public Vector3 startRotation = Vector3.zero;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        if (!fromCurrentRotation)
        {
            if (useLocalRotation)
                t.localEulerAngles = startRotation;
            else
                t.eulerAngles = startRotation;
        }

        var tween = useLocalRotation
            ? t.DOLocalRotate(targetRotation, duration, rotateMode)
            : t.DORotate(targetRotation, duration, rotateMode);

        return tween.SetEase(ease).SetDelay(delay);
    }
}
