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
        // 인덱스 0 = ★★★(시간), 1 = ★★☆(횟수), 2 = ★☆☆(클리어)
        [SerializeField] private CanvasGroup[] starRowGroups;
        [SerializeField] private TMP_Text[]    starThresholdTexts;

        [Header("Buttons")]
        [SerializeField] private RcButton retryButton;
        [SerializeField] private RcButton lobbyButton;
        [SerializeField] private RcButton closeButton;

        private const float ActiveAlpha = 1.0f;
        private const float DimAlpha    = 0.4f;

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

        // Row 0 (★★★): 시간 조건 / Row 1 (★★☆): 횟수 조건 / Row 2 (★☆☆): CLEAR
        public void SetStarConditions(int moveCountThreshold, float timeThreshold)
        {
            if (starThresholdTexts == null) return;

            if (starThresholdTexts.Length > 0 && starThresholdTexts[0] != null)
                starThresholdTexts[0].text = timeThreshold > 0f ? $"{timeThreshold:0}SEC" : "-";

            if (starThresholdTexts.Length > 1 && starThresholdTexts[1] != null)
                starThresholdTexts[1].text = moveCountThreshold > 0 ? $"{moveCountThreshold} MOVES" : "-";

            if (starThresholdTexts.Length > 2 && starThresholdTexts[2] != null)
                starThresholdTexts[2].text = "CLEAR";
        }

        public void RefreshStarHighlights(int currentMove, float elapsedTime, int moveCountThreshold, float timeThreshold)
        {
            if (starRowGroups == null) return;

            // Row 0: 시간 조건
            if (starRowGroups.Length > 0 && starRowGroups[0] != null)
                starRowGroups[0].alpha = (timeThreshold > 0f && elapsedTime <= timeThreshold) ? ActiveAlpha : DimAlpha;

            // Row 1: 횟수 조건
            if (starRowGroups.Length > 1 && starRowGroups[1] != null)
                starRowGroups[1].alpha = (moveCountThreshold > 0 && currentMove <= moveCountThreshold) ? ActiveAlpha : DimAlpha;

            // Row 2: 클리어 — 항상 달성 가능
            if (starRowGroups.Length > 2 && starRowGroups[2] != null)
                starRowGroups[2].alpha = ActiveAlpha;
        }
    }
}
