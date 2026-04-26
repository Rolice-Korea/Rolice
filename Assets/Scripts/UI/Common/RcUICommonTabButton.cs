using System;
using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 여러 패널(가방, 샵 등)에서 공용으로 사용할 수 있는 탭 버튼 위젯.
    ///RcUIStateMachine을 통해 Selected/Unselected 상태 비주얼을 관리함.
    /// </summary>
    public class RcUICommonTabButton : RcUIWidget
    {
        [Header("UI References")]
        [SerializeField] private RcButton button;
        [SerializeField] private TMP_Text labelText;
        
        [Header("State")]
        [SerializeField] private RcUIStateMachine stateMachine;
        
        private int tabIndex;
        private Action<int> onSelected;

        public int TabIndex => tabIndex;

        public override void Initialize()
        {
            if (button != null)
                button.OnClick += HandleClick;
        }

        public override void Cleanup()
        {
            if (button != null)
                button.OnClick -= HandleClick;
            
            onSelected = null;
        }

        public void Setup(int index, string label, Action<int> callback)
        {
            tabIndex = index;
            onSelected = callback;
            
            if (labelText != null)
                labelText.text = label;
        }

        public void SetSelected(bool isSelected)
        {
            if (stateMachine != null)
                stateMachine.SetState(isSelected ? "Selected" : "Unselected");
        }

        private void HandleClick()
        {
            onSelected?.Invoke(tabIndex);
        }
    }
}
