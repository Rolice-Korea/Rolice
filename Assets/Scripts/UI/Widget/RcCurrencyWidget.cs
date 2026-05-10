using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcCurrencyWidget : RcUIWidget
    {
        [SerializeField] private RcButton button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;

        public event Action OnClicked;

        public override void Initialize()
        {
            button.OnClick += HandleClick;
        }

        public override void Cleanup()
        {
            button.OnClick -= HandleClick;
            OnClicked = null;
        }

        public void SetAmount(int amount)
        {
            if (amountText != null)
                amountText.text = amount.ToString();
        }

        public void SetIcon(Sprite sprite)
        {
            if (iconImage != null)
                iconImage.sprite = sprite;
        }

        private void HandleClick() => OnClicked?.Invoke();
    }
}
