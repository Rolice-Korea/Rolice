using Cysharp.Threading.Tasks;
using Engine.UI;
using TMPro;
using UnityEngine;

namespace Rolice.UI
{
    public class RcRetryDialogPanel : RcUIPanel<string>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private RcButton retryButton;

        private UniTaskCompletionSource _tcs;

        protected override void OnOpen()
        {
            messageText.text = Data;
            _tcs = new UniTaskCompletionSource();
            retryButton.OnClick += OnRetryClicked;
        }

        protected override void OnBeforeClose()
        {
            retryButton.OnClick -= OnRetryClicked;
        }

        public UniTask WaitForRetryAsync() => _tcs.Task;

        private void OnRetryClicked() => _tcs.TrySetResult();
    }
}
