using System;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcLobbySettingsPanel : RcUIPanel
    {
        [Header("UI References")]
        [SerializeField] private RcButton backdropButton;
        [SerializeField] private RcButton closeButton;

        [Header("Screen Mode")]
        [SerializeField] private RcButton landscapeButton;
        [SerializeField] private RcButton portraitButton;

        private RcLobbySettingsPresenter presenter;

        public event Action OnCloseClicked;
        public event Action<RcScreenMode> OnScreenModeChanged;

        protected override void Awake()
        {
            base.Awake();

            if (backdropButton != null)
                backdropButton.OnClick += () => OnCloseClicked?.Invoke();

            if (closeButton != null)
                closeButton.OnClick += () => OnCloseClicked?.Invoke();

            if (landscapeButton != null)
                landscapeButton.OnClick += () => OnScreenModeChanged?.Invoke(RcScreenMode.Landscape);

            if (portraitButton != null)
                portraitButton.OnClick += () => OnScreenModeChanged?.Invoke(RcScreenMode.Portrait);
        }

        protected override void OnOpen()
        {
            presenter = new RcLobbySettingsPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }
    }
}
