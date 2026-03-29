using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcGameHudPanel : RcUIPanel
    {
        [Header("Move Count")]
        [SerializeField] private TMP_Text moveCountText;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerText;

        [Header("Pause")]
        [SerializeField] private RcButton pauseButton;

        public RcButton PauseButton => pauseButton;

        [Header("Controls")]
        [SerializeField] private RcSwipeArea swipeArea;
        [SerializeField] private RcButton rotateLeftButton;
        [SerializeField] private RcButton rotateRightButton;

        public RcSwipeArea SwipeArea        => swipeArea;
        public RcButton    RotateLeftButton  => rotateLeftButton;
        public RcButton    RotateRightButton => rotateRightButton;

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

        private void Update()
        {
            if (timerText == null) return;
            var mgr = RcGameRuleManager.Instance;
            if (mgr == null || !mgr.IsInitialized) return;

            float t = mgr.ElapsedTime;
            int minutes = (int)(t / 60f);
            int seconds = (int)(t % 60f);
            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        public void SetMoveCount(int count)
        {
            if (moveCountText != null)
                moveCountText.text = $"{count}";
        }
    }
}
