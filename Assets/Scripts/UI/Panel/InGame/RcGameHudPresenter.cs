using Engine.UI;

namespace Rolice.UI
{
    public class RcGameHudPresenter : RcUIPresenter<RcGameHudPanel>
    {
        protected override void OnInitialize()
        {
            RcGameEvents.Instance.Subscribe(RcGameEvent.TurnChanged, OnTurnChanged);
            Panel.PauseButton.OnClick       += OnPauseClicked;
            Panel.RotateLeftButton.OnDown    += OnRotateLeftDown;
            Panel.RotateLeftButton.OnUp      += OnRotateLeftUp;
            Panel.RotateRightButton.OnDown   += OnRotateRightDown;
            Panel.RotateRightButton.OnUp     += OnRotateRightUp;

            Panel.SetMoveCount(0);
        }

        protected override void OnDispose()
        {
            RcGameEvents.Instance.Unsubscribe(RcGameEvent.TurnChanged, OnTurnChanged);
            Panel.PauseButton.OnClick       -= OnPauseClicked;
            Panel.RotateLeftButton.OnDown    -= OnRotateLeftDown;
            Panel.RotateLeftButton.OnUp      -= OnRotateLeftUp;
            Panel.RotateRightButton.OnDown   -= OnRotateRightDown;
            Panel.RotateRightButton.OnUp     -= OnRotateRightUp;

            RcInputController.Instance?.StopCameraRotate();
        }

        private void OnPauseClicked()      => RcUIManager.Instance.Open<RcPausePanel>();
        private void OnRotateLeftDown()    => RcInputController.Instance?.StartCameraRotate(1);
        private void OnRotateLeftUp()      => RcInputController.Instance?.StopCameraRotate();
        private void OnRotateRightDown()   => RcInputController.Instance?.StartCameraRotate(-1);
        private void OnRotateRightUp()     => RcInputController.Instance?.StopCameraRotate();

        private void OnTurnChanged(int currentTurn)
        {
            Panel.SetMoveCount(currentTurn);
        }
    }
}
