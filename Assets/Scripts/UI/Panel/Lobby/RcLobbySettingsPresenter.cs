using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcLobbySettingsPresenter : RcUIPresenter<RcLobbySettingsPanel>
    {
        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleCloseClicked;
            Panel.OnMasterVolumeChanged += HandleMasterVolumeChanged;
            Panel.OnSfxVolumeChanged += HandleSfxVolumeChanged;
            Panel.OnBgmVolumeChanged += HandleBgmVolumeChanged;
            
            LoadSettingsData();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleCloseClicked;
            Panel.OnMasterVolumeChanged -= HandleMasterVolumeChanged;
            Panel.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
            Panel.OnBgmVolumeChanged -= HandleBgmVolumeChanged;
        }

        private void LoadSettingsData()
        {
            // TODO: 실제 계정 시스템 연동 시 수정
            string mockInfo = "Guest User (Lv.1)";
            Panel.SetAccountInfo(mockInfo);

            // TODO: RcSoundManager.Instance.GetVolume() 등 실제 데이터 연동
            Panel.SetMasterVolume(1.0f);
            Panel.SetSfxVolume(0.8f);
            Panel.SetBgmVolume(0.6f);
        }

        private void HandleCloseClicked()
        {
            Panel.Close();
        }

        private void HandleMasterVolumeChanged(float volume)
        {
            // TODO: 사운드 매니저 연동
        }

        private void HandleSfxVolumeChanged(float volume)
        {
            // TODO: 사운드 매니저 연동
        }

        private void HandleBgmVolumeChanged(float volume)
        {
            // TODO: 사운드 매니저 연동
        }
    }
}
