using System;
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
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _stageNumberText;
        [SerializeField] private Image _glowFrame;
        [SerializeField] private Image[] _starImages;

        [Header("Text Alpha")]
        [SerializeField, Range(0f, 1f)] private float _lockedTextAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _unlockedTextAlpha = 0.7f;

        [Header("Glow / Star Alpha")]
        [SerializeField, Range(0f, 1f)] private float _glowDimmedAlpha = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _starDimmedAlpha = 0.2f;

        private int _stageNumber;
        private RcStageState _state;
        private int _stars;
        private bool _isSelected;

        public int StageNumber => _stageNumber;
        public RcStageState State => _state;
        public event Action<int> OnStageSelected;

        public override void Initialize()
        {
            _button.onClick.AddListener(HandleClick);
            if (_glowFrame != null) _glowFrame.raycastTarget = false;
            if (_stageNumberText != null) _stageNumberText.raycastTarget = false;
        }

        public override void Cleanup()
        {
            _button.onClick.RemoveListener(HandleClick);
            OnStageSelected = null;
        }

        public void SetData(int stageNumber, RcStageState state, int stars = 0)
        {
            _stageNumber = stageNumber;
            _state = state;
            _stars = Mathf.Clamp(stars, 0, 3);
            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            UpdateStageNumber();
            UpdateStars();
            UpdateGlow();
            _button.interactable = _state != RcStageState.Locked;
        }

        private void UpdateStageNumber()
        {
            if (_stageNumberText == null) return;

            _stageNumberText.text = _stageNumber.ToString();
            SetAlpha(_stageNumberText, GetTextAlpha());
        }

        private void UpdateStars()
        {
            if (_starImages == null) return;

            for (int i = 0; i < _starImages.Length; i++)
            {
                if (_starImages[i] == null) continue;

                bool isEarned = _state == RcStageState.Cleared && i < _stars;
                SetAlpha(_starImages[i], isEarned ? 1f : _starDimmedAlpha);
            }
        }

        private void UpdateGlow()
        {
            if (_glowFrame == null) return;

            bool show = _state != RcStageState.Locked;
            _glowFrame.gameObject.SetActive(show);

            if (show)
                SetAlpha(_glowFrame, _isSelected ? 1f : _glowDimmedAlpha);
        }

        private float GetTextAlpha()
        {
            if (_isSelected || _state == RcStageState.Cleared) return 1f;
            return _state == RcStageState.Locked ? _lockedTextAlpha : _unlockedTextAlpha;
        }

        private void HandleClick()
        {
            if (_state == RcStageState.Locked) return;
            OnStageSelected?.Invoke(_stageNumber);
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
