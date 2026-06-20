using UnityEngine;

namespace Engine.UI
{
    /// <summary>
    /// RcUIStateMachine과 RcTweenAnimator를 연결하는 브릿지.
    /// StateMachine의 상태명과 TweenAnimator의 시퀀스명이 일치하면 자동으로 재생.
    /// </summary>
    public class RcUIVisualStateAdapter : MonoBehaviour
    {
        [SerializeField] private RcUIStateMachine stateMachine;
        [SerializeField] private RcTweenAnimator tweenAnimator;

        private void OnEnable()
        {
            stateMachine.OnStateChanged += HandleStateChanged;

            // 패널 재활성화 시 현재 상태 즉시 적용
            if (!string.IsNullOrEmpty(stateMachine.CurrentState))
                tweenAnimator?.PlayInstant(Resolve(stateMachine.CurrentState));
        }

        private void OnDisable()
        {
            stateMachine.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(string from, string to)
        {
            tweenAnimator?.Play(Resolve(to));
        }

        // 정의되지 않은 시퀀스는 "Normal"로 폴백 (Owned·Locked 등 미완성 상태 대응)
        private string Resolve(string state) =>
            tweenAnimator != null && tweenAnimator.HasSequence(state) ? state : "Normal";
    }
}
