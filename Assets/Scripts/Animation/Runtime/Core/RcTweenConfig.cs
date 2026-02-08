using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public abstract class RcTweenConfig
{
    public enum SequenceMode { Append, Join }

    [Header("Sequence")]
    public SequenceMode sequenceMode = SequenceMode.Append;

    [Header("Target")]
    public Transform target;

    [Header("Common")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutQuad;
    public float delay = 0f;

    public abstract Tween CreateTween(Transform defaultTarget);
}
