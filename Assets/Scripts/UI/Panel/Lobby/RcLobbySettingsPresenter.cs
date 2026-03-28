using Engine.UI;
using Rolice.Data;
using Rolice.System;

namespace Rolice.UI
{
    public class RcLobbySettingsPresenter : RcUIPresenter<RcLobbySettingsPanel>
    {
        private bool stageSelectWasOpen;

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleCloseClicked;
            Panel.OnScreenModeChanged += HandleScreenModeChanged;

            stageSelectWasOpen = RcUIManager.Instance.IsOpen<RcStageSelectPanel>();
            if (stageSelectWasOpen)
                RcUIManager.Instance.Close<RcStageSelectPanel>();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleCloseClicked;
            Panel.OnScreenModeChanged -= HandleScreenModeChanged;
        }

        private void HandleCloseClicked()
        {
            if (stageSelectWasOpen)
                RcUIManager.Instance.Open<RcStageSelectPanel>();

            Panel.Close();
        }

        private void HandleScreenModeChanged(RcScreenMode mode)
        {
            // 설정 저장
            RcGameSettingsData.Current.SetScreenMode(mode);

            // 화면 방향 적용
            RcScreenOrientationApplier.Apply(mode);
        }
    }
}
