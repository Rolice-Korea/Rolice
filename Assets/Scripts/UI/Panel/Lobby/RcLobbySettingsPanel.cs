using System;
using Engine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcLobbySettingsPanel : RcUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Button backdropButton;

        private RcLobbySettingsPresenter presenter;

        public event Action OnCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (backdropButton != null)
                backdropButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
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
