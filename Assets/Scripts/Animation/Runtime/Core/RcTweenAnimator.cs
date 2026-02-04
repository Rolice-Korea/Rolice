using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

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

    [SerializeField] private List<AnimationSequence> _sequences = new();
    [SerializeField] private bool _playOnEnable = false;
    [SerializeField] private RcOnDisableBehaviorType _onDisableBehaviorType = RcOnDisableBehaviorType.Rewind;

    public Action OnComplete;
    public Action OnStart;

    private Sequence _currentSequence;
    private string _currentSequenceName;

    private void OnEnable()
    {
        if (_playOnEnable)
            Play("Default");
    }

    private void OnDisable()
    {
        if (_currentSequence == null || !_currentSequence.IsActive())
            return;

        switch (_onDisableBehaviorType)
        {
            case RcOnDisableBehaviorType.Kill:
                _currentSequence.Kill();
                break;
            case RcOnDisableBehaviorType.Rewind:
                _currentSequence.Rewind();
                _currentSequence.Kill();
                break;
            case RcOnDisableBehaviorType.Complete:
                _currentSequence.Complete();
                break;
        }

        _currentSequence = null;
    }

    public void Play(string sequenceName = "Default")
    {
        var sequence = _sequences.Find(s => s.name == sequenceName);
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

        _currentSequenceName = sequenceName;
        OnStart?.Invoke();

        _currentSequence = DOTween.Sequence();

        foreach (var animation in sequence.animations)
        {
            if (animation == null) continue;

            var tween = animation.CreateTween(transform);
            if (tween == null) continue;

            if (animation.sequenceMode == RcTweenConfig.SequenceMode.Append)
                _currentSequence.Append(tween);
            else
                _currentSequence.Join(tween);
        }

        if (sequence.loop)
            _currentSequence.SetLoops(sequence.loopCount, sequence.loopType);

        _currentSequence.OnComplete(() => OnComplete?.Invoke());
        _currentSequence.Play();
    }

    public void Stop()
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
            _currentSequence = null;
        }
        _currentSequenceName = null;
    }

    public void Pause()
    {
        if (_currentSequence != null && _currentSequence.IsActive() && _currentSequence.IsPlaying())
            _currentSequence.Pause();
    }

    public void Resume()
    {
        if (_currentSequence != null && _currentSequence.IsActive() && !_currentSequence.IsPlaying())
            _currentSequence.Play();
    }

    public void Restart()
    {
        if (!string.IsNullOrEmpty(_currentSequenceName))
            Play(_currentSequenceName);
        else if (_currentSequence != null && _currentSequence.IsActive())
            _currentSequence.Restart();
    }

    public bool IsPlaying() =>
        _currentSequence != null && _currentSequence.IsActive() && _currentSequence.IsPlaying();

    public void PlayOnClick() => Play("OnClick");
    public void PlayOnEnter() => Play("OnEnter");
    public void PlayOnExit() => Play("OnExit");
    public void PlayOnOpen() => Play("OnOpen");
    public void PlayOnClose() => Play("OnClose");
}
