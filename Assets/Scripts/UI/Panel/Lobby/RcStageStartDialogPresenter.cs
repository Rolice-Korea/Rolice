using Engine.UI;

namespace Rolice.UI
{
    public class RcStageStartDialogPresenter : RcUIPresenter<RcStageStartDialog>
    {
        protected override void OnInitialize()
        {
            Panel.StartButton.onClick.AddListener(HandleStart);
            Panel.CancelButton.onClick.AddListener(HandleCancel);
            PopulateView(Panel.Data);
        }

        protected override void OnDispose()
        {
            Panel.StartButton.onClick.RemoveListener(HandleStart);
            Panel.CancelButton.onClick.RemoveListener(HandleCancel);
        }

        private void PopulateView(RcStageStartDialogData data)
        {
            Panel.SetTitle(data.StageName);
            PopulateConditions(data.StarThresholds);
            Panel.SetProgress(data.CurrentStars);
        }

        private void PopulateConditions(int[] thresholds)
        {
            if (thresholds == null) return;
            for (int i = 0; i < thresholds.Length; i++)
            {
                int starCount = thresholds.Length - i;
                Panel.SetConditionRow(i, starCount, thresholds[i]);
            }
        }

        private void HandleStart()
        {
            int stageNumber = Panel.Data.StageNumber;
            Panel.Close();
            RcGameFlowManager.Instance.GoToStage(stageNumber);
        }

        private void HandleCancel()
        {
            Panel.Close();
        }
    }
}
