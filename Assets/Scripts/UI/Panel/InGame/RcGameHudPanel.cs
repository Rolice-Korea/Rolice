using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcGameHudPanel : RcUIPanel
    {
        [Header("Turn")]
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private RcTweenAnimator animatorGameOver;

        private RcGameHudPresenter presenter;

        protected override void OnOpen()
        {
            presenter = new RcGameHudPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void SetRemainingTurns(int remaining)
        {
            if (turnText == null) return;

            turnText.text = $"{remaining}";
        }

        public void PlayGameOverAnimation()
        {
            if (animatorGameOver != null && !animatorGameOver.IsPlaying())
            {
                animatorGameOver.OnComplete = OnGameOverAnimationComplete;
                animatorGameOver.Play("GameOver");
            }
        }

        private void OnGameOverAnimationComplete()
        {
            RcGameResultManager.Instance.ShowGameOverResult();
        }
    }
}
