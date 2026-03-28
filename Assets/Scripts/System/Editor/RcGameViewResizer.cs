#if UNITY_EDITOR
using System;
using System.Reflection;
using Rolice;
using Rolice.System;
using UnityEditor;
using UnityEngine;

namespace Rolice.Editor
{
    /// <summary>
    /// 에디터 Play 모드에서 화면 모드 변경 시 GameView 해상도를 실제로 전환한다.
    /// </summary>
    [InitializeOnLoad]
    public static class RcGameViewResizer
    {
        private const string SizeLandscape = "1920x1080";
        private const string SizePortrait  = "1080x1920";

        private const int LandscapeWidth  = 1920;
        private const int LandscapeHeight = 1080;
        private const int PortraitWidth   = 1080;
        private const int PortraitHeight  = 1920;

        static RcGameViewResizer()
        {
            RcScreenOrientationApplier.OnModeApplied += OnModeApplied;
        }

        private static void OnModeApplied(RcScreenMode mode)
        {
            int w = mode == RcScreenMode.Landscape ? LandscapeWidth : PortraitWidth;
            int h = mode == RcScreenMode.Landscape ? LandscapeHeight : PortraitHeight;

            SetGameViewSize(w, h);
        }

        private static void SetGameViewSize(int width, int height)
        {
            try
            {
                var asm = typeof(UnityEditor.Editor).Assembly;

                // GameViewSizes 싱글톤 가져오기
                var sizesType  = asm.GetType("UnityEditor.GameViewSizes");
                var singleton  = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instance   = singleton.GetProperty("instance").GetValue(null);

                // 현재 플랫폼의 사이즈 그룹 가져오기
                var getGroupType = sizesType.GetMethod("get_currentGroupType");
                var groupType    = getGroupType.Invoke(instance, null);
                var getGroup     = sizesType.GetMethod("GetGroup");
                var group        = getGroup.Invoke(instance, new[] { groupType });

                // 그룹에서 일치하는 해상도 찾기
                var groupTypeRef = asm.GetType("UnityEditor.GameViewSizeGroup");
                var getTotal     = groupTypeRef.GetMethod("GetTotalCount");
                var getSize      = groupTypeRef.GetMethod("GetGameViewSize");
                int total        = (int)getTotal.Invoke(group, null);

                var sizeType = asm.GetType("UnityEditor.GameViewSize");
                var wProp    = sizeType.GetProperty("width");
                var hProp    = sizeType.GetProperty("height");

                int foundIndex = -1;

                for (int i = 0; i < total; i++)
                {
                    var size = getSize.Invoke(group, new object[] { i });
                    int sw   = (int)wProp.GetValue(size);
                    int sh   = (int)hProp.GetValue(size);

                    if (sw == width && sh == height)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                // 없으면 커스텀 사이즈 추가
                if (foundIndex == -1)
                {
                    var sizeTypeEnum    = asm.GetType("UnityEditor.GameViewSizeType");
                    var fixedResolution = Enum.Parse(sizeTypeEnum, "FixedResolution");
                    var ctor = sizeType.GetConstructor(new[]
                    {
                        sizeTypeEnum, typeof(int), typeof(int), typeof(string)
                    });
                    var newSize = ctor.Invoke(new object[]
                    {
                        fixedResolution, width, height, $"{width}x{height}"
                    });

                    var addCustom = groupTypeRef.GetMethod("AddCustomSize");
                    addCustom.Invoke(group, new[] { newSize });

                    foundIndex = total;
                }

                // GameView에서 해당 인덱스 선택
                var gameViewType      = asm.GetType("UnityEditor.GameView");
                var gameView          = EditorWindow.GetWindow(gameViewType);
                var selectedSizeIndex = gameViewType.GetProperty(
                    "selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                selectedSizeIndex.SetValue(gameView, foundIndex);
                gameView.Repaint();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameViewResizer] GameView 해상도 변경 실패: {e.Message}");
            }
        }
    }
}
#endif
