using Engine.UI;

namespace Rolice.UI
{
    public class RcGameResultPresenter : RcUIPresenter<RcGameResultPanel>
    {
        protected override void OnInitialize()
        {
            InitializeDisplay(Panel.Data);

            Panel.RetryButton.onClick.AddListener(OnRetryClicked);
            Panel.NextLevelButton.onClick.AddListener(OnNextLevelClicked);
            Panel.LobbyButton.onClick.AddListener(OnLobbyClicked);
        }

        protected override void OnDispose()
        {
            Panel.RetryButton.onClick.RemoveListener(OnRetryClicked);
            Panel.NextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            Panel.LobbyButton.onClick.RemoveListener(OnLobbyClicked);
        }

        private void InitializeDisplay(RcGameResultData data)
        {
            Panel.SetResultTitle(data.IsVictory);
            Panel.SetMoveCount(data.TurnUsed);
            Panel.SetStars(data.StarCount);
            Panel.SetRetryButtonVisible(!data.IsVictory);
            Panel.SetNextButtonVisible(data.IsVictory && data.HasNextStage);
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
