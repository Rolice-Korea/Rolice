using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI에서 개별 판매 아이템을 표시하는 슬롯 위젯.
    /// </summary>
    public class RcUIShopItem : RcUIWidget
    {
        [Header("UI References")]
        [SerializeField] private RcButton button;
        [SerializeField] private Image skinImage;
        [SerializeField] private TMP_Text priceText;
        
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

        public void Setup(int index, Color previewColor, string price, Action<int> callback)
        {
            itemIndex = index;
            onClicked = callback;

            if (skinImage != null)
                skinImage.color = previewColor;

            if (priceText != null)
                priceText.text = price;
        }

        /// <summary>
        /// 아이템의 시각적 상태를 설정한다.
        /// </summary>
        /// <param name="isSelected">현재 선택된 아이템인지</param>
        /// <param name="isOwned">이미 보유한 아이템인지 (1회성 구매 완료 시)</param>
        public void SetState(bool isSelected, bool isOwned)
        {
            if (stateMachine != null)
            {
                if (isOwned)
                    stateMachine.SetState("Owned");
                else
                    stateMachine.SetState(isSelected ? "Selected" : "Normal");
            }

            // 보유한 아이템은 선택 불가 (이미 구매 완료)
            if (button != null)
                button.Interactable = !isOwned;
        }

        private void HandleClick()
        {
            onClicked?.Invoke(itemIndex);
        }
    }
}
