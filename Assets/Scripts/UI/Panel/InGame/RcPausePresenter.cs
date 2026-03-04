using Engine.UI;

namespace Rolice.UI
{
    public class RcPausePresenter : RcUIPresenter<RcPausePanel>
    {
        private int[] starThresholds;

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
            int currentTurn = RcGameRuleManager.Instance.CurrentTurn;
            Panel.SetCurrentTurn(currentTurn);

            int stageNumber = RcGameContext.SelectedStageNumber;
            if (stageNumber <= 0) return;

            var levelData = RcProgressManager.Instance.StageDatabase.GetStage(stageNumber);
            if (levelData == null) return;

            starThresholds = levelData.StageInfo.StarThresholds;
            Panel.SetStarConditions(starThresholds);
            Panel.RefreshStarHighlights(currentTurn, starThresholds);
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
