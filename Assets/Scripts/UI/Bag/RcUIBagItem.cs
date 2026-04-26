using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    /// <summary>
    /// 가방 UI에서 개별 스킨 아이템을 표시하는 슬롯 위젯.
    /// </summary>
    public class RcUIBagItem : RcUIWidget
    {
        [Header("UI References")]
        [SerializeField] private RcButton button;
        [SerializeField] private Image skinImage;
        
        [Header("State")]
        [SerializeField] private RcUIStateMachine stateMachine;

        private int itemIndex;
        private Action<int> onClicked;

        public int ItemIndex => itemIndex;

        public override void Initialize()
        {
            if (button != null)
                button.OnClick += HandleClick;
        }

        public override void Cleanup()
        {
            if (button != null)
                button.OnClick -= HandleClick;
            
            onClicked = null;
        }

        public void Setup(int index, Color previewColor, Action<int> callback)
        {
            itemIndex = index;
            onClicked = callback;

            if (skinImage != null)
                skinImage.color = previewColor;
        }

        public void SetState(bool isSelected, bool isLocked)
        {
            if (stateMachine == null) return;

            if (isLocked)
                stateMachine.SetState("Locked");
            else
                stateMachine.SetState(isSelected ? "Selected" : "Normal");
            
            // 버튼 상호작용성 제어 (잠금 상태에서도 선택은 가능하게 할 수 있으나 여기서는 기본적으로 잠금 시 선택 불가 처리)
            if (button != null)
                button.Interactable = !isLocked;
        }

        private void HandleClick()
        {
            onClicked?.Invoke(itemIndex);
        }
    }
}
