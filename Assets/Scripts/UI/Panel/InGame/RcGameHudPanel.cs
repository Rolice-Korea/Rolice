using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcGameHudPanel : RcUIPanel
    {
        [Header("Move Count")]
        [SerializeField] private TMP_Text moveCountText;

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

        public void SetMoveCount(int count)
        {
            if (moveCountText != null)
                moveCountText.text = $"{count}";
        }
    }
}
