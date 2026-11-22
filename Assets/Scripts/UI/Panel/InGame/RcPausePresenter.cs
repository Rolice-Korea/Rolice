using Engine.UI;

namespace Rolice.UI
{
    public class RcPausePresenter : RcUIPresenter<RcPausePanel>
    {
        protected override void OnInitialize()
        {
            RcPauseManager.Instance.Pause();
            RcUIManager.Instance.Close<RcGameHudPanel>();

            InitializeDisplay();

            Panel.RetryButton.OnClick += OnRetryClicked;
            Panel.LobbyButton.OnClick += OnLobbyClicked;
            Panel.CloseButton.OnClick += OnCloseClicked;
        }

        protected override void OnDispose()
        {
            Panel.RetryButton.OnClick -= OnRetryClicked;
            Panel.LobbyButton.OnClick -= OnLobbyClicked;
            Panel.CloseButton.OnClick -= OnCloseClicked;

            RcPauseManager.Instance.Resume();
        }

        private void InitializeDisplay()
        {
            int currentMove = RcGameRuleManager.Instance.CurrentTurn;
            Panel.SetCurrentMove(currentMove);
            Panel.HideStarRows();
        }

        private void OnRetryClicked() => RcGameFlowManager.Instance.RetryStage();
        private void OnLobbyClicked() => RcGameFlowManager.Instance.GoToLobby();
        private void OnCloseClicked()
        {
            RcUIManager.Instance.Close<RcPausePanel>();
            RcUIManager.Instance.Open<RcGameHudPanel>();
        }
    }
}
