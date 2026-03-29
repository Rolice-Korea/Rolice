using Engine.UI;

namespace Rolice.UI
{
    public class RcGameHudPresenter : RcUIPresenter<RcGameHudPanel>
    {
        protected override void OnInitialize()
        {
            RcGameEvents.Instance.Subscribe(RcGameEvent.TurnChanged, OnTurnChanged);
            Panel.PauseButton.OnClick      += OnPauseClicked;
            Panel.RotateLeftButton.OnClick  += OnRotateLeftClicked;
            Panel.RotateRightButton.OnClick += OnRotateRightClicked;

            Panel.SetMoveCount(0);
        }

        protected override void OnDispose()
        {
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.TurnChanged, OnTurnChanged);
            Panel.PauseButton.OnClick      -= OnPauseClicked;
            Panel.RotateLeftButton.OnClick  -= OnRotateLeftClicked;
            Panel.RotateRightButton.OnClick -= OnRotateRightClicked;
        }

        private void OnPauseClicked()       => RcUIManager.Instance.Open<RcPausePanel>();
        private void OnRotateLeftClicked()  => RcInputController.Instance?.TriggerRotate(1);
        private void OnRotateRightClicked() => RcInputController.Instance?.TriggerRotate(-1);

        private void OnTurnChanged(int currentTurn)
        {
            Panel.SetMoveCount(currentTurn);
        }
    }
}
