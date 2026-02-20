using System;
using DG.Tweening;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public enum RcStageState
    {
        Locked,
        Unlocked,
        Cleared
    }

    public class RcStageItemWidget : RcUIWidget
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text stageNumberText;
        [SerializeField] private Image glowFrame;
        [SerializeField] private Image[] starImages;

        [Header("Text Alpha")]
        [SerializeField, Range(0f, 1f)] private float lockedTextAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float unlockedTextAlpha = 0.7f;

        [Header("Glow / Star Alpha")]
        [SerializeField, Range(0f, 1f)] private float glowDimmedAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float starDimmedAlpha = 0.2f;

        private static readonly Color GlowColorSelected = new Color(0.1f, 0.9f, 1f, 1f);
        private static readonly Color GlowColorIdle = Color.white;

        private int stageNumber;
        private RcStageState state;
        private int stars;
        private bool isSelected;
        private CanvasGroup canvasGroup;
        private RcUIHDRImage glowHDRImage;
        private Tweener pulseTween;
        private RcTweenAnimator tweenAnimator;

        public int StageNumber => stageNumber;
        public RcStageState State => state;
        public float CarouselAlpha => canvasGroup != null ? canvasGroup.alpha : 1f;
        public event Action<int> OnStageSelected;

        public override void Initialize()
        {
            button.onClick.AddListener(HandleClick);
            if (glowFrame != null)
            {
                glowFrame.raycastTarget = false;
                glowHDRImage = glowFrame.GetComponent<RcUIHDRImage>();
            }
            if (stageNumberText != null) stageNumberText.raycastTarget = false;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            tweenAnimator = GetComponent<RcTweenAnimator>();
        }

        public void PlaySelectAnimation()
        {
            tweenAnimator?.Play("OnSelect");
        }
        
        public void EvaluateCarousel(float t)
        {
            tweenAnimator?.Evaluate(t, "CarouselState");
        }

        public override void Cleanup()
        {
            pulseTween?.Kill();
            button.onClick.RemoveListener(HandleClick);
            OnStageSelected = null;
        }

        public void SetData(int stageNumber, RcStageState state, int stars = 0)
        {
            this.stageNumber = stageNumber;
            this.state = state;
            this.stars = Mathf.Clamp(stars, 0, 3);
            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public void SetCarouselAlpha(float alpha)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
        }

        public void SetCarouselVisual(float scale, float alpha)
        {
            transform.localScale = Vector3.one * scale;
            SetCarouselAlpha(alpha);
        }

        private void UpdateVisual()
        {
            UpdateStageNumber();
            UpdateStars();
            UpdateGlow();
            button.interactable = state != RcStageState.Locked;
        }

        private void UpdateStageNumber()
        {
            if (stageNumberText == null) return;

            stageNumberText.text = stageNumber.ToString();
            SetAlpha(stageNumberText, GetTextAlpha());
        }

        private void UpdateStars()
        {
            if (starImages == null) return;

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;

                bool isEarned = state == RcStageState.Cleared && i < stars;
                SetAlpha(starImages[i], isEarned ? 1f : starDimmedAlpha);
            }
        }

        private void UpdateGlow()
        {
            if (glowFrame == null) return;

            bool show = state != RcStageState.Locked;
            glowFrame.gameObject.SetActive(show);

            pulseTween?.Kill();
            pulseTween = null;

            if (!show) return;

            if (isSelected)
            {
                SetAlpha(glowFrame, 1f);
                if (glowHDRImage != null)
                {
                    glowHDRImage.HDRColor = GlowColorSelected;
                    glowHDRImage.Intensity = 3.5f;
                    pulseTween = DOVirtual.Float(3.5f, 7f, 1.4f, v => glowHDRImage.Intensity = v)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                SetAlpha(glowFrame, glowDimmedAlpha);
                if (glowHDRImage != null)
                {
                    glowHDRImage.HDRColor = GlowColorIdle;
                    glowHDRImage.Intensity = 1f;
                }
            }
        }

        private float GetTextAlpha()
        {
            if (isSelected || state == RcStageState.Cleared) return 1f;
            return state == RcStageState.Locked ? lockedTextAlpha : unlockedTextAlpha;
        }

        private void HandleClick()
        {
            if (state == RcStageState.Locked) return;
            OnStageSelected?.Invoke(stageNumber);
        }

        private static void SetAlpha(Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        private static void SetAlpha(TMP_Text text, float alpha)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
