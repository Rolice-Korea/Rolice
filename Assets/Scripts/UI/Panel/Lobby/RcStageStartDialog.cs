using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public class RcStageStartDialog : RcUIPanel<RcStageStartDialogData>
    {
        [Serializable]
        private class ConditionRow
        {
            public Image[] starImages;
            public TMP_Text turnText;
        }

        [Header("Title")]
        [SerializeField] private TMP_Text titleText;

        [Header("Star Conditions")]
        [SerializeField] private ConditionRow[] conditionRows;

        [Header("Current Progress")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image[] currentStarImages;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;

        private static readonly Color StarEarnedColor = Color.white;
        private static readonly Color StarDimColor = new Color(0.33f, 0.33f, 0.33f, 0.4f);

        private RcStageStartDialogPresenter presenter;

        protected override void OnOpen()
        {
            presenter = new RcStageStartDialogPresenter();
            presenter.Bind(this);
        }

        protected override void OnBeforeClose()
        {
            presenter?.Unbind();
            presenter = null;
        }

        public void SetTitle(string title) => titleText.text = title;

        public void SetConditionRow(int index, int starCount, int turnThreshold)
        {
            if (index >= conditionRows.Length) return;
            var row = conditionRows[index];
            row.turnText.text = $"{turnThreshold} TURNS";
            for (int i = 0; i < row.starImages.Length; i++)
                row.starImages[i].color = i < starCount ? StarEarnedColor : StarDimColor;
        }

        public void SetProgress(int currentStars)
        {
            progressText.text = currentStars > 0
                ? $"<color=#FFF700>CLEARED</color>\nBEST: {currentStars} STAR"
                : "NOT CLEARED";

            UpdateStarDisplay(currentStars);
        }

        private void UpdateStarDisplay(int stars)
        {
            if (currentStarImages == null) return;
            for (int i = 0; i < currentStarImages.Length; i++)
            {
                if (currentStarImages[i] == null) continue;
                currentStarImages[i].color = i < stars ? StarEarnedColor : StarDimColor;
            }
        }

        public Button StartButton => startButton;
        public Button CancelButton => cancelButton;
    }
}
