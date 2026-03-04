using System;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    public class RcLobbySettingsPanel : RcUIPanel
    {
        [Header("UI References")]
        [SerializeField] private RcButton backdropButton;

        private RcLobbySettingsPresenter presenter;

        public event Action OnCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (backdropButton != null)
                backdropButton.OnClick += () => OnCloseClicked?.Invoke();
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
