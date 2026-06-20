using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 슬롯의 시각 상태.
    /// 우선순위(presenter 결정): Selected > Owned > Locked > Normal.
    /// 문자열 이름이 곧 RcUIStateMachine 상태 키 (애니메이션은 에디터에서 바인딩).
    /// </summary>
    public enum RcShopItemState
    {
        Normal,
        Selected,
        Owned,
        Locked,
    }

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

        public void Setup(int index, Color previewColor, Sprite iconSprite, string price, Action<int> callback)
        {
            itemIndex = index;
            onClicked = callback;

            if (skinImage != null)
            {
                if (iconSprite != null)
                {
                    skinImage.sprite = iconSprite;
                    skinImage.color = Color.white;
                }
                else
                {
                    skinImage.sprite = null;
                    skinImage.color = previewColor;
                }
            }

            if (priceText != null)
                priceText.text = price;
        }

        /// <summary>
        /// 아이템의 시각적 상태를 설정한다.
        /// 보유/잠금 슬롯도 선택은 가능하다 — 우측 정보 패널에서 "보유 중"·"★N 필요"를 보여주기 위함.
        /// 실제 구매 가능 여부(Buy 버튼)는 presenter가 별도로 게이팅한다.
        /// </summary>
        public void SetState(RcShopItemState state)
        {
            if (stateMachine != null)
                stateMachine.SetState(state.ToString());
        }

        private void HandleClick()
        {
            onClicked?.Invoke(itemIndex);
        }
    }
}
