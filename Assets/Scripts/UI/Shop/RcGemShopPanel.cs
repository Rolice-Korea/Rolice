using Engine.UI;

namespace Rolice.UI
{
    // TODO: 잼 상점 팝업 — 상단: 광고 시청으로 잼 +1 목록 / 하단: IAP 잼 구매 목록
    public class RcGemShopPanel : RcUIPanel
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
