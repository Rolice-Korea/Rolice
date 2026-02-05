using UnityEngine;

namespace Engine.UI
{
    public class RcTestPresenter : RcUIPresenter<RcTestPanel>
    {
        protected override void OnInitialize()
        {
            Panel.SetTitle("test");
            Panel.CloseButton.onClick.AddListener(OnCloseClicked);

            Debug.Log("[RcTestPresenter] 바인딩 완료");
        }

        private void OnCloseClicked()
        {
            RcUIManager.Instance.CloseCurrent();
        }

        protected override void OnDispose()
        {
            Panel.CloseButton.onClick.RemoveListener(OnCloseClicked);

            Debug.Log("[RcTestPresenter] 해제 완료");
        }
    }
}
