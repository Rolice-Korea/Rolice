using System;
using Rolice;
using UnityEngine;

namespace Rolice.System
{
    /// <summary>
    /// 화면 방향을 실제로 적용하는 유틸리티.
    /// 에디터에서는 OnModeApplied 이벤트를 통해 Editor 스크립트가 GameView를 조작한다.
    /// </summary>
    public static class RcScreenOrientationApplier
    {
        /// <summary>
        /// 에디터 전용 — Editor 스크립트가 구독하여 GameView 해상도를 변경한다.
        /// </summary>
        public static event Action<RcScreenMode> OnModeApplied;

        public static void Apply(RcScreenMode mode)
        {
            Screen.orientation = mode switch
            {
                RcScreenMode.Landscape => ScreenOrientation.LandscapeLeft,
                RcScreenMode.Portrait  => ScreenOrientation.Portrait,
                _                      => ScreenOrientation.Portrait,
            };

            switch (mode)
            {
                case RcScreenMode.Landscape:
                    Screen.autorotateToPortrait           = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                    Screen.autorotateToLandscapeLeft      = true;
                    Screen.autorotateToLandscapeRight     = true;
                    break;

                case RcScreenMode.Portrait:
                    Screen.autorotateToPortrait           = true;
                    Screen.autorotateToPortraitUpsideDown = false;
                    Screen.autorotateToLandscapeLeft      = false;
                    Screen.autorotateToLandscapeRight     = false;
                    break;
            }

            OnModeApplied?.Invoke(mode);

            Debug.Log($"[ScreenOrientation] 화면 모드 변경: {mode}");
        }
    }
}
