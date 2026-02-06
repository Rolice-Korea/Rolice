using System;
using Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rolice.UI
{
    public enum RcStageState
    {
        Locked,     // 잠금
        Unlocked,   // 해제 (플레이 가능)
        Cleared     // 클리어됨
    }

    /// <summary>
    /// 개별 스테이지 아이템 위젯
    /// </summary>
    public class RcStageItemWidget : RcUIWidget
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _stageNumberText;
        [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private GameObject[] _starIcons;  // 별 아이콘 3개

        [Header("Visual Settings")]
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _unlockedColor = Color.white;
        [SerializeField] private Color _clearedColor = new Color(1f, 0.9f, 0.5f);

        private int _stageNumber;
        private RcStageState _state;
        private int _stars;

        public event Action<int> OnStageSelected;

        public int StageNumber => _stageNumber;
        public RcStageState State => _state;

        public override void Initialize()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        public override void Cleanup()
        {
            _button.onClick.RemoveListener(OnButtonClick);
            OnStageSelected = null;
        }

        /// <summary>
        /// 스테이지 데이터 바인딩
        /// </summary>
        public void SetData(int stageNumber, RcStageState state, int stars = 0)
        {
            _stageNumber = stageNumber;
            _state = state;
            _stars = Mathf.Clamp(stars, 0, 3);

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            // 스테이지 번호
            if (_stageNumberText != null)
                _stageNumberText.text = _stageNumber.ToString();

            // 잠금 아이콘
            if (_lockedIcon != null)
                _lockedIcon.SetActive(_state == RcStageState.Locked);

            // 별 아이콘
            UpdateStars();

            // 버튼 상호작용
            _button.interactable = _state != RcStageState.Locked;

            // 색상
            UpdateColor();
        }

        private void UpdateStars()
        {
            if (_starIcons == null) return;

            // 잠금 상태면 별 숨김
            bool showStars = _state == RcStageState.Cleared;

            for (int i = 0; i < _starIcons.Length; i++)
            {
                if (_starIcons[i] != null)
                {
                    // 클리어 상태일 때만 획득한 별만큼 표시
                    _starIcons[i].SetActive(showStars && i < _stars);
                }
            }
        }

        private void UpdateColor()
        {
            var image = _button.GetComponent<Image>();
            if (image == null) return;

            image.color = _state switch
            {
                RcStageState.Locked => _lockedColor,
                RcStageState.Unlocked => _unlockedColor,
                RcStageState.Cleared => _clearedColor,
                _ => _unlockedColor
            };
        }

        private void OnButtonClick()
        {
            if (_state == RcStageState.Locked) return;

            OnStageSelected?.Invoke(_stageNumber);
        }
    }
}
