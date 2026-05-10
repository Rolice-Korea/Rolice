using Engine.UI;
using Rolice.Define;
using Rolice.System;

namespace Rolice.UI
{
    public class RcCurrencyHudPresenter : RcUIPresenter<RcCurrencyHudPanel>
    {
        protected override void OnInitialize()
        {
            Panel.HeartWidget.OnClicked += OnHeartClicked;
            Panel.GemWidget.OnClicked   += OnGemClicked;

            RcPlayerState.Instance.OnProgressChanged += Refresh;
            Refresh();
        }

        protected override void OnDispose()
        {
            Panel.HeartWidget.OnClicked -= OnHeartClicked;
            Panel.GemWidget.OnClicked   -= OnGemClicked;

            RcPlayerState.Instance.OnProgressChanged -= Refresh;
        }

        private void Refresh()
        {
            var state = RcPlayerState.Instance;
            Panel.HeartWidget.SetAmount(state.GetCurrency(RcCurrencyId.Heart.ToKey()));
            Panel.GemWidget.SetAmount(state.GetCurrency(RcCurrencyId.Gem.ToKey()));
        }

        private void OnHeartClicked() => RcUIManager.Instance.Open<RcHeartShopPanel>();
        private void OnGemClicked()   => RcUIManager.Instance.Open<RcGemShopPanel>();
    }
}
