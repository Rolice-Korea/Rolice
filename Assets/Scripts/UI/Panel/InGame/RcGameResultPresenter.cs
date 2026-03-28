using Engine.UI;

namespace Rolice.UI
{
    public class RcGameResultPresenter : RcUIPresenter<RcGameResultPanel>
    {
        protected override void OnInitialize()
        {
            InitializeDisplay(Panel.Data);

            Panel.RetryButton.OnClick += OnRetryClicked;
            Panel.NextLevelButton.OnClick += OnNextLevelClicked;
            Panel.LobbyButton.OnClick += OnLobbyClicked;
        }

        protected override void OnDispose()
        {
            Panel.RetryButton.OnClick -= OnRetryClicked;
            Panel.NextLevelButton.OnClick -= OnNextLevelClicked;
            Panel.LobbyButton.OnClick -= OnLobbyClicked;
        }

        private void InitializeDisplay(RcGameResultData data)
        {
            Panel.SetResultTitle(data.IsVictory);
            Panel.SetMoveCount(data.MoveCount);
            Panel.SetStars(data.StarCount);
            Panel.SetRetryButtonVisible(false);
            Panel.SetNextButtonVisible(data.HasNextStage);
        }

        private void OnRetryClicked()
        {
            RcGameFlowManager.Instance.RetryStage();
        }

        private void OnNextLevelClicked()
        {
            RcGameFlowManager.Instance.GoToNextStage();
        }

        private void OnLobbyClicked()
        {
            RcGameFlowManager.Instance.GoToLobby();
        }
    }
}
