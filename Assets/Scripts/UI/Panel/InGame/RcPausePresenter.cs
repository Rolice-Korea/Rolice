using Engine.UI;

namespace Rolice.UI
{
    public class RcPausePresenter : RcUIPresenter<RcPausePanel>
    {
        private int   moveCountThreshold;
        private float timeThreshold;

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
            int   currentMove = RcGameRuleManager.Instance.CurrentTurn;
            float elapsed     = RcGameRuleManager.Instance.ElapsedTime;

            Panel.SetCurrentMove(currentMove);

            int stageNumber = RcGameContext.SelectedStageNumber;
            if (stageNumber <= 0) return;

            var levelData = RcProgressManager.Instance.StageDatabase.GetStage(stageNumber);
            if (levelData == null) return;

            moveCountThreshold = levelData.StageInfo.MoveCountThreshold;
            timeThreshold      = levelData.StageInfo.TimeThreshold;

            Panel.SetStarConditions(moveCountThreshold, timeThreshold);
            Panel.RefreshStarHighlights(currentMove, elapsed, moveCountThreshold, timeThreshold);
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
