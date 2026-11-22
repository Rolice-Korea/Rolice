using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcPausePanel : RcUIPanel
    {
        [Header("Current Move")]
        [SerializeField] private TMP_Text currentTurnText;

        [Header("Star Rows")]
        [SerializeField] private CanvasGroup[] starRowGroups;
        [SerializeField] private TMP_Text[]    starThresholdTexts;

        [Header("Buttons")]
        [SerializeField] private RcButton retryButton;
        [SerializeField] private RcButton lobbyButton;
        [SerializeField] private RcButton closeButton;

        public RcButton RetryButton => retryButton;
        public RcButton LobbyButton => lobbyButton;
        public RcButton CloseButton => closeButton;

        private RcPausePresenter presenter;

        protected override void OnOpen()
        {
            presenter = new RcPausePresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void SetCurrentMove(int moveCount)
        {
            if (currentTurnText != null)
                currentTurnText.text = $"{moveCount} MOVE";
        }

        public void HideStarRows()
        {
            if (starRowGroups == null) return;

            for (int i = 0; i < starRowGroups.Length; i++)
            {
                if (starRowGroups[i] != null)
                    starRowGroups[i].gameObject.SetActive(false);
            }
        }
    }
}
