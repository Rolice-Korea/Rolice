using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcGameHudPanel : RcUIPanel
    {
        [Header("Turn")]
        [SerializeField] private TMP_Text turnText;

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
    }
}
