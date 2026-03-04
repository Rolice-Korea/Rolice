using System;
using UnityEngine;

namespace Engine.UI
{
    public class RcUIStateMachine : MonoBehaviour
    {
        private string currentState;

        public string CurrentState => currentState;

        /// <summary>
        /// 상태 전환 시 발행. (from, to)
        /// </summary>
        public event Action<string, string> OnStateChanged;

        /// <summary>
        /// 상태 전환. 동일 상태면 무시, 이벤트 발행.
        /// </summary>
        public void SetState(string state)
        {
            if (currentState == state) return;
            var prev = currentState;
            currentState = state;
            OnStateChanged?.Invoke(prev, state);
        }

        /// <summary>
        /// 초기 상태 설정. 이벤트 발행 없음 (애니메이션 없이 조용히 설정).
        /// </summary>
        public void InitState(string state)
        {
            currentState = state;
        }
    }
}
