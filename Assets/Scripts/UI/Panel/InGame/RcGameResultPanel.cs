using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button lobbyButton;

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

        public async void SetStars(int count)
        {
            for (int i = 0; i < starGlows.Length; i++)
            {
                starGlows[i].SetActive(i < count);
            }

            if (starGlowParticlePrefab != null && particleFactory != null)
            {
                for (int i = 0; i < count; i++)
                {
                    await UniTask.Delay((int)(starPopDelay * 1000f));
                    PlayStarParticle(i);
                }
            }
        }

        private void PlayStarParticle(int starIndex)
        {
            if (starIndex >= starGlows.Length) return;

            var starTransform = starGlows[starIndex].transform;
            var effect = particleFactory.Get($"star_{starIndex}", starGlowParticlePrefab);

            effect.transform.SetParent(starTransform);
            effect.transform.localPosition = Vector3.zero;

            effect.PlayAsync().Forget();
        }

        public void SetRetryButtonVisible(bool visible)
        {
            retryButton.gameObject.SetActive(visible);
        }

        public void SetNextButtonVisible(bool visible)
        {
            nextLevelButton.gameObject.SetActive(visible);
        }

        public Button RetryButton => retryButton;
        public Button NextLevelButton => nextLevelButton;
        public Button LobbyButton => lobbyButton;
    }
}
