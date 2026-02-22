using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcLobbySettingsPanel : RcUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text accountInfoText; // 추후 계정 정보 표시용
        [SerializeField] private Slider masterVolumeSlider; // 볼륨 조절 슬라이더
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        private RcLobbySettingsPresenter presenter;
        
        public event Action OnCloseClicked;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action<float> OnBgmVolumeChanged;

        protected override void Awake()
        {
            base.Awake();
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener((val) => OnMasterVolumeChanged?.Invoke(val));
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener((val) => OnSfxVolumeChanged?.Invoke(val));
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.AddListener((val) => OnBgmVolumeChanged?.Invoke(val));
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            presenter = new RcLobbySettingsPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            base.OnBeforeClose();
            
            presenter?.Unbind();
            presenter = null;
        }
        
        public void SetAccountInfo(string info)
        {
            if (accountInfoText != null)
            {
                accountInfoText.text = info;
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(volume);
            }
        }
        
        public void SetSfxVolume(float volume)
        {
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(volume);
            }
        }
        
        public void SetBgmVolume(float volume)
        {
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.SetValueWithoutNotify(volume);
            }
        }
    }
}
