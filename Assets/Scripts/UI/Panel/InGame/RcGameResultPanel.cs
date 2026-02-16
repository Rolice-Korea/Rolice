using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcGameResultPanel : RcUIPanel<RcGameResultData>
    {
        [Header("Result")]
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text moveText;

        [Header("Stars")]
        [SerializeField] private GameObject[] stars;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button lobbyButton;

        private RcGameResultPresenter presenter;

        protected override void OnOpen()
        {
            presenter = new RcGameResultPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void SetResultTitle(string title)
        {
            resultTitleText.text = title;
        }

        public void SetMoveCount(int turnUsed)
        {
            moveText.text = $"Move : {turnUsed}";
        }

        public void SetStars(int count)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(i < count);
            }
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
