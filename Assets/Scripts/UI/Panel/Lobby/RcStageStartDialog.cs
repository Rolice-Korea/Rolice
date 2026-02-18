using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    /// <summary>
    /// 스테이지 시작 확인 다이얼로그
    /// 스테이지 정보 표시 및 시작/취소 선택
    /// </summary>
    public class RcStageStartDialog : RcUIPanel
    {
        [Header("Title")]
        [SerializeField] private TMP_Text titleText;

        [Header("Star Info")]
        [SerializeField] private TMP_Text starInfoText;

        [Header("Current Progress")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image[] currentStarImages;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;

    private int selectedStageNumber = -1;

        public event Action<int> OnStartStage;
        public event Action OnCanceled;

        protected override void OnOpen()
        {
            startButton?.onClick.AddListener(HandleStartClick);
            cancelButton?.onClick.AddListener(HandleCancelClick);
        }

        protected override void OnBeforeClose()
        {
            startButton?.onClick.RemoveListener(HandleStartClick);
            cancelButton?.onClick.RemoveListener(HandleCancelClick);
        }

        public void SetStageInfo(int stageNumber, string stageName, int[] starThresholds, int currentStars)
        {
            selectedStageNumber = stageNumber;

            titleText.text = stageName;

            if (starThresholds != null && starThresholds.Length > 0)
            {
                string info = "STAR CONDITIONS\n";
                for (int i = 0; i < starThresholds.Length; i++)
                {
                    int stars = starThresholds.Length - i;
                    info += $"<color=#FFF700>{stars} STAR</color>: {starThresholds[i]} turns\n";
                }
                starInfoText.text = info;
            }

            if (currentStars > 0)
            {
                progressText.text = $"<color=#FFF700>CLEARED</color>\nBest: {currentStars} STAR";
                UpdateStarDisplay(currentStars);
            }
            else
            {
                progressText.text = "Not Cleared";
                UpdateStarDisplay(0);
            }
        }

        private void UpdateStarDisplay(int stars)
        {
            if (currentStarImages == null) return;

            for (int i = 0; i < currentStarImages.Length; i++)
            {
                if (currentStarImages[i] == null) continue;
                bool isEarned = i < stars;
                SetAlpha(currentStarImages[i], isEarned ? 1f : 0.2f);
            }
        }

        private void HandleStartClick()
        {
            int stageToStart = selectedStageNumber;
            Close();
            OnStartStage?.Invoke(stageToStart);
        }

        private void HandleCancelClick()
        {
            Close();
            OnCanceled?.Invoke();
        }

        private static void SetAlpha(Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

    }
}
