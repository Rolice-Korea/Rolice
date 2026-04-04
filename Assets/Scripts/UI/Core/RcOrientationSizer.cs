using Rolice;
using Rolice.System;
using UnityEngine;

namespace Engine.UI
{
    /// <summary>
    /// 가로/세로 모드에 따라 RectTransform 크기를 전환하는 컴포넌트.
    /// PANEL_view 등 고정 크기 다이얼로그에 부착.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RcOrientationSizer : MonoBehaviour
    {
        [SerializeField] private Vector2 landscapeSize = new Vector2(580, 750);
        [SerializeField] private Vector2 portraitSize  = new Vector2(580, 920);

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            RcScreenOrientationApplier.OnModeApplied += Apply;
        }

        private void Start()
        {
            bool isPortrait = Screen.height > Screen.width;
            Apply(isPortrait ? RcScreenMode.Portrait : RcScreenMode.Landscape);
        }

        private void OnDestroy()
        {
            RcScreenOrientationApplier.OnModeApplied -= Apply;
        }

        private void Apply(RcScreenMode mode)
        {
            _rect.sizeDelta = mode == RcScreenMode.Portrait ? portraitSize : landscapeSize;
        }
    }
}
