using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RcTweenAnimator : MonoBehaviour
{
    [Serializable]
    public class AnimationSequence
    {
        public string name = "Default";

        [SerializeReference]
        public List<RcTweenConfig> animations = new();

        [Header("Loop")]
        public bool loop = false;
        public int loopCount = -1;
        public LoopType loopType = LoopType.Restart;
    }

    [SerializeField] private List<AnimationSequence> sequences = new();
    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private RcOnDisableBehaviorType onDisableBehaviorType = RcOnDisableBehaviorType.Rewind;

    public Action OnComplete;
    public Action OnStart;

    private Sequence currentSequence;
    private string currentSequenceName;

    private void OnEnable()
    {
        if (playOnEnable)
            Play("Default");
    }

    private void OnDisable()
    {
        if (currentSequence == null || !currentSequence.IsActive())
            return;

        switch (onDisableBehaviorType)
        {
            case RcOnDisableBehaviorType.Kill:
                currentSequence.Kill();
                break;
            case RcOnDisableBehaviorType.Rewind:
                currentSequence.Rewind();
                currentSequence.Kill();
                break;
            case RcOnDisableBehaviorType.Complete:
                currentSequence.Complete();
                break;
        }

        currentSequence = null;
    }

    public void Play(string sequenceName = "Default")
    {
        var sequence = sequences.Find(s => s.name == sequenceName);
        if (sequence == null)
        {
            Debug.LogWarning($"[RcTweenAnimator] Sequence '{sequenceName}' not found.", this);
            return;
        }

        if (sequence.animations == null || sequence.animations.Count == 0)
        {
            Debug.LogWarning($"[RcTweenAnimator] Sequence '{sequenceName}' has no animations.", this);
            return;
        }

        Stop();

        currentSequenceName = sequenceName;
        OnStart?.Invoke();

        currentSequence = DOTween.Sequence();

        foreach (var animation in sequence.animations)
        {
            if (animation == null) continue;

            var tween = animation.CreateTween(transform);
            if (tween == null) continue;

            if (animation.sequenceMode == RcTweenConfig.SequenceMode.Append)
                currentSequence.Append(tween);
            else
                currentSequence.Join(tween);
        }

        if (sequence.loop)
            currentSequence.SetLoops(sequence.loopCount, sequence.loopType);

        currentSequence.OnComplete(() => OnComplete?.Invoke());
        currentSequence.Play();
    }

    public void Stop()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }
        currentSequenceName = null;
    }

    public void Pause()
    {
        if (currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying())
            currentSequence.Pause();
    }

    public void Resume()
    {
        if (currentSequence != null && currentSequence.IsActive() && !currentSequence.IsPlaying())
            currentSequence.Play();
    }

    public void Restart()
    {
        if (!string.IsNullOrEmpty(currentSequenceName))
            Play(currentSequenceName);
        else if (currentSequence != null && currentSequence.IsActive())
            currentSequence.Restart();
    }

    public bool IsPlaying() =>
        currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying();

    public void PlayOnClick() => Play("OnClick");
    public void PlayOnEnter() => Play("OnEnter");
    public void PlayOnExit() => Play("OnExit");
    public void PlayOnOpen() => Play("OnOpen");
    public void PlayOnClose() => Play("OnClose");
}
