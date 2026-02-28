using Engine.UI;

namespace Rolice.UI
{
    public class RcLobbySettingsPresenter : RcUIPresenter<RcLobbySettingsPanel>
    {
        private bool stageSelectWasOpen;

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleCloseClicked;

            stageSelectWasOpen = RcUIManager.Instance.IsOpen<RcStageSelectPanel>();
            if (stageSelectWasOpen)
                RcUIManager.Instance.Close<RcStageSelectPanel>();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleCloseClicked;
        }

        private void HandleCloseClicked()
        {
            if (stageSelectWasOpen)
                RcUIManager.Instance.Open<RcStageSelectPanel>();

            Panel.Close();
        }
    }
}
