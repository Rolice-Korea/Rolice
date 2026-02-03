using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RcShakeConfig : RcTweenConfig
{
    public enum ShakeType { Position, Rotation, Scale }

    [Header("Shake")]
    public ShakeType shakeType = ShakeType.Position;
    public float strength = 0.5f;
    public int vibrato = 10;
    [Range(0, 90)] public float randomness = 90f;
    public bool fadeOut = true;

    public override Tween CreateTween(Transform defaultTarget)
    {
        var t = target != null ? target : defaultTarget;
        if (t == null) return null;

        var tween = shakeType switch
        {
            ShakeType.Position => t.DOShakePosition(duration, strength, vibrato, randomness, fadeOut),
            ShakeType.Rotation => t.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut),
            ShakeType.Scale => t.DOShakeScale(duration, strength, vibrato, randomness, fadeOut),
            _ => null
        };

        return tween?.SetEase(ease).SetDelay(delay);
    }
}
