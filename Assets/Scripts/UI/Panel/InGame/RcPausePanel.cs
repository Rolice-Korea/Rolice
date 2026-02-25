using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcPausePanel : RcUIPanel
    {
        [Header("Current Turn")]
        [SerializeField] private TMP_Text currentTurnText;

        [Header("Star Rows")]
        // 인덱스 0 = ★★★, 1 = ★★☆, 2 = ★☆☆
        [SerializeField] private CanvasGroup[] starRowGroups;
        [SerializeField] private TMP_Text[] starThresholdTexts;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button closeButton;

        private const float ActiveAlpha = 1.0f;
        private const float DimAlpha    = 0.4f;

        public Button RetryButton => retryButton;
        public Button LobbyButton => lobbyButton;
        public Button CloseButton => closeButton;

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

        public void SetCurrentTurn(int turn)
        {
            if (currentTurnText != null)
                currentTurnText.text = $"현재  {turn}턴";
        }

        // thresholds[0] = ★★★ 기준턴, thresholds[1] = ★★☆ 기준턴, ★☆☆는 항상 "클리어"
        public void SetStarConditions(int[] thresholds)
        {
            if (starThresholdTexts == null) return;

            for (int i = 0; i < starThresholdTexts.Length; i++)
            {
                if (starThresholdTexts[i] == null) continue;

                if (i < 2)
                {
                    starThresholdTexts[i].text = (thresholds != null && i < thresholds.Length)
                        ? $"{thresholds[i]}턴 이하"
                        : "-";
                }
                else
                {
                    starThresholdTexts[i].text = "클리어";
                }
            }
        }

        // 현재 턴 기준으로 달성 가능한 별 행을 밝게, 나머지는 어둡게
        public void RefreshStarHighlights(int currentTurn, int[] thresholds)
        {
            if (starRowGroups == null) return;

            for (int i = 0; i < starRowGroups.Length; i++)
            {
                if (starRowGroups[i] == null) continue;

                bool achievable;
                if (i < 2)
                {
                    achievable = thresholds != null && i < thresholds.Length
                        && currentTurn <= thresholds[i];
                }
                else
                {
                    // ★☆☆: 클리어만 하면 항상 달성 가능
                    achievable = true;
                }

                starRowGroups[i].alpha = achievable ? ActiveAlpha : DimAlpha;
            }
        }
    }
}
