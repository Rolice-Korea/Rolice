using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    public class RcTestPanel : RcUIPanel
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _closeButton;

        private RcTestPresenter _presenter;

        public Button CloseButton => _closeButton;

        public void SetTitle(string text) => _titleText.text = text;

        protected override void OnOpen()
        {
            _presenter = new RcTestPresenter();
            _presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            _presenter.Unbind();
            _presenter = null;
        }
    }
}
