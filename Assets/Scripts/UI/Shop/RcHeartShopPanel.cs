using Engine.UI;

namespace Rolice.UI
{
    // TODO: 하트 충전 팝업 — 젬을 소모해 하트를 충전하는 UI
    public class RcHeartShopPanel : RcUIPanel
    {
        [UnityEngine.SerializeField] private RcButton closeButton;

        protected override void OnOpen()
        {
            if (closeButton != null)
                closeButton.OnClick += Close;
        }

        protected override void OnBeforeClose()
        {
            if (closeButton != null)
                closeButton.OnClick -= Close;
        }

        private void Close() => base.Close();
    }
}
