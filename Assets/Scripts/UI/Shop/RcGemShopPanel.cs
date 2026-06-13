using System;
using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 잼 구매 팝업.
    ///   상단: 광고 시청 → 잼 +1 (하루 1회, RcAdRewardService — 실동작)
    ///   하단: IAP 잼 팩 (결제 인프라 미구현 → "준비 중" 비활성 placeholder)
    /// HUD 잼 위젯 클릭 시 진입. 뷰만 담당하고 로직은 RcGemShopPresenter가 처리한다.
    /// </summary>
    public class RcGemShopPanel : RcUIPanel
    {
        [Header("Buttons")]
        [SerializeField] private RcButton closeButton;
        [SerializeField] private RcButton adButton;

        [Header("IAP Placeholder")]
        [SerializeField] private RcButton  iapButton;     // 준비 중 — 비활성 고정
        [SerializeField] private TMP_Text  iapStatusText; // "준비 중"

        [Header("Texts")]
        [SerializeField] private TMP_Text gemBalanceText;
        [SerializeField] private TMP_Text adStatusText;

        public event Action OnCloseClicked;
        public event Action OnAdClicked;

        private RcGemShopPresenter presenter;

        protected override void Awake()
        {
            base.Awake();
            if (closeButton != null) closeButton.OnClick += () => OnCloseClicked?.Invoke();
            if (adButton    != null) adButton.OnClick    += () => OnAdClicked?.Invoke();

            // IAP는 아직 인프라가 없어 항상 비활성 + 안내 문구
            if (iapButton != null) iapButton.Interactable = false;
            if (iapStatusText != null) iapStatusText.text = "준비 중";
        }

        protected override void OnOpen()
        {
            presenter = new RcGemShopPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void RequestClose() => Close();

        public void SetGemBalance(int amount)
        {
            if (gemBalanceText != null)
                gemBalanceText.text = amount.ToString();
        }

        public void SetAdStatusText(string text)
        {
            if (adStatusText != null)
                adStatusText.text = text;
        }

        public void SetAdInteractable(bool interactable)
        {
            if (adButton != null)
                adButton.Interactable = interactable;
        }
    }
}
