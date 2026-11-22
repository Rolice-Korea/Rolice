using Engine.UI;

namespace Rolice.UI
{
    public class RcGameResultPresenter : RcUIPresenter<RcGameResultPanel>
    {
        protected override void OnInitialize()
        {
            InitializeDisplay(Panel.Data);

            Panel.RetryButton.OnClick += OnRetryClicked;
            Panel.LobbyButton.OnClick += OnLobbyClicked;
        }

        protected override void OnDispose()
        {
            Panel.RetryButton.OnClick -= OnRetryClicked;
            Panel.LobbyButton.OnClick -= OnLobbyClicked;
        }

        private void InitializeDisplay(RcGameResultData data)
        {
            Panel.SetResultTitle(data.IsVictory);
            Panel.SetMoveCount(data.MoveCount);
            Panel.HideStars();
            Panel.SetRetryButtonVisible(true);
            Panel.SetNextButtonVisible(false);
        }

        private void OnRetryClicked()
        {
            RcGameFlowManager.Instance.RetryStage();
        }

        private void OnLobbyClicked()
        {
            RcGameFlowManager.Instance.GoToLobby();
        }
    }
}
