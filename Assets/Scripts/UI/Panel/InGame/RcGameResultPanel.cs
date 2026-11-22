using Engine.UI;
using TMPro;
using UnityEngine;
using Rolice.Particle;

namespace Rolice.UI
{
    public class RcGameResultPanel : RcUIPanel<RcGameResultData>
    {
        [Header("Result")]
        [SerializeField] private GameObject victoryText;
        [SerializeField] private GameObject failedText;
        [SerializeField] private TMP_Text moveText;

        [Header("Stars")]
        [SerializeField] private GameObject[] starGlows;
        [SerializeField] private GameObject starGlowParticlePrefab;
        [SerializeField] private float starPopDelay = 0.3f;

        [Header("Buttons")]
        [SerializeField] private RcButton retryButton;
        [SerializeField] private RcButton nextLevelButton;
        [SerializeField] private RcButton lobbyButton;

        private RcGameResultPresenter presenter;
        private RcParticleEffectFactory particleFactory;

        protected override void OnOpen()
        {
            particleFactory = new RcParticleEffectFactory();
            presenter = new RcGameResultPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
            particleFactory?.Clear();
            particleFactory = null;
        }

        public void SetResultTitle(bool isVictory)
        {
            victoryText.SetActive(isVictory);
            failedText.SetActive(!isVictory);
        }

        public void SetMoveCount(int turnUsed)
        {
            moveText.text = $"Move : {turnUsed}";
        }

        public void HideStars()
        {
            if (starGlows == null) return;

            for (int i = 0; i < starGlows.Length; i++)
            {
                if (starGlows[i] == null) continue;
                starGlows[i].SetActive(false);
                if (starGlows[i].transform.parent != null)
                    starGlows[i].transform.parent.gameObject.SetActive(false);
            }
        }

        public void SetRetryButtonVisible(bool visible)
        {
            retryButton.gameObject.SetActive(visible);
        }

        public void SetNextButtonVisible(bool visible)
        {
            nextLevelButton.gameObject.SetActive(visible);
        }

        public RcButton RetryButton => retryButton;
        public RcButton NextLevelButton => nextLevelButton;
        public RcButton LobbyButton => lobbyButton;
    }
}
