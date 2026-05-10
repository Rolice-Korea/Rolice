using Engine.UI;

namespace Rolice.UI
{
    public class RcCurrencyHudPanel : RcUIPanel
    {
        [UnityEngine.SerializeField] private RcCurrencyWidget heartWidget;
        [UnityEngine.SerializeField] private RcCurrencyWidget gemWidget;

        public RcCurrencyWidget HeartWidget => heartWidget;
        public RcCurrencyWidget GemWidget   => gemWidget;

        private RcCurrencyHudPresenter presenter;

        protected override void OnOpen()
        {
            heartWidget.Initialize();
            gemWidget.Initialize();

            presenter = new RcCurrencyHudPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;

            heartWidget.Cleanup();
            gemWidget.Cleanup();
        }
    }
}
