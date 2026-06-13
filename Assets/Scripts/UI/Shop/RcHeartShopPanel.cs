using System;
using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 하트 충전 팝업 (잼 소모 → 하트).
    /// HUD 하트 위젯 클릭 시 진입. 뷰만 담당하고 로직은 RcHeartShopPresenter가 처리한다.
    /// </summary>
    public class RcHeartShopPanel : RcUIPanel
    {
        [Header("Buttons")]
        [SerializeField] private RcButton closeButton;
        [SerializeField] private RcButton refillButton;

        [Header("Texts")]
        [SerializeField] private TMP_Text heartCountText;
        [SerializeField] private TMP_Text gemBalanceText;
        [SerializeField] private TMP_Text costText;

        [Header("Config")]
        [SerializeField] private int gemCostPerHeart = 10;

        public int GemCostPerHeart => gemCostPerHeart;

        public event Action OnCloseClicked;
        public event Action OnRefillClicked;

        private RcHeartShopPresenter presenter;

        protected override void Awake()
        {
            base.Awake();
            if (closeButton  != null) closeButton.OnClick  += () => OnCloseClicked?.Invoke();
            if (refillButton != null) refillButton.OnClick += () => OnRefillClicked?.Invoke();
        }

        protected override void OnOpen()
        {
            presenter = new RcHeartShopPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void RequestClose() => Close();

        public void SetHeartCount(int current, int max)
        {
            if (heartCountText != null)
                heartCountText.text = $"{current} / {max}";
        }

        public void SetGemBalance(int amount)
        {
            if (gemBalanceText != null)
                gemBalanceText.text = amount.ToString();
        }

        public void SetCostText(string text)
        {
            if (costText != null)
                costText.text = text;
        }

        public void SetRefillInteractable(bool interactable)
        {
            if (refillButton != null)
                refillButton.Interactable = interactable;
        }
    }
}
