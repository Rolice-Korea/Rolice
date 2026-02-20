using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RcPunchConfig : RcTweenConfig
{
    public enum PunchType { Position, Rotation, Scale }

    [Header("Punch")]
    public PunchType punchType = PunchType.Scale;
    public Vector3 punch = Vector3.one * 0.1f;
    public int vibrato = 5;
    [Range(0f, 1f)] public float elasticity = 0.5f;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        Tween tween = punchType switch
        {
            PunchType.Position => t.DOPunchPosition(punch, duration, vibrato, elasticity),
            PunchType.Rotation => t.DOPunchRotation(punch, duration, vibrato, elasticity),
            PunchType.Scale    => t.DOPunchScale(punch, duration, vibrato, elasticity),
            _                  => null
        };

        return tween?.SetEase(ease).SetDelay(delay);
    }
}
