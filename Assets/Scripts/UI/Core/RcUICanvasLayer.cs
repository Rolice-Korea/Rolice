using System.Collections.Generic;
using Rolice;
using Rolice.System;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    public class RcUICanvasLayer
    {
        private readonly Transform root;
        private Camera camera;
        private readonly Dictionary<RcUILayer, Canvas> canvases = new();
        private readonly List<CanvasScaler> scalers = new();

        public RcUICanvasLayer(Transform root, Camera camera = null)
        {
            this.root = root;
            this.camera = camera;
            RcScreenOrientationApplier.OnModeApplied += OnOrientationChanged;
        }

        public void UpdateCamera(Camera camera)
        {
            this.camera = camera;
            foreach (var kvp in canvases)
            {
                if (kvp.Value == null) continue;

                if (camera != null && kvp.Key != RcUILayer.AbsoluteOverlay)
                {
                    kvp.Value.renderMode = RenderMode.ScreenSpaceCamera;
                    kvp.Value.worldCamera = camera;
                    kvp.Value.planeDistance = camera.nearClipPlane + 0.01f;
                }
                // 카메라가 null이어도 Overlay로 전환하지 않음
                // 씬 전환 중 렌더 모드 변경으로 깜빡임 방지
            }
        }

        public Transform GetRoot(RcUILayer layer)
        {
            return GetOrCreateCanvas(layer).transform;
        }

        public Canvas GetCanvas(RcUILayer layer)
        {
            return GetOrCreateCanvas(layer);
        }

        private Canvas GetOrCreateCanvas(RcUILayer layer)
        {
            if (canvases.TryGetValue(layer, out var existing))
                return existing;

            var go = new GameObject($"Canvas_{layer}");
            go.transform.SetParent(root, false);

            var canvas = go.AddComponent<Canvas>();

            if (camera != null && layer != RcUILayer.AbsoluteOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = camera.nearClipPlane + 0.01f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            canvas.sortingOrder = (int)layer;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            ApplyScalerForCurrentOrientation(scaler);
            scalers.Add(scaler);

            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<CanvasGroup>();

            canvases[layer] = canvas;
            return canvas;
        }

        public void SetVisible(bool visible)
        {
            foreach (var kvp in canvases)
            {
                if (kvp.Value == null) continue;
                var cg = kvp.Value.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = visible ? 1f : 0f;
            }
        }

        private void OnOrientationChanged(RcScreenMode mode)
        {
            foreach (var scaler in scalers)
            {
                if (scaler != null)
                    ApplyScalerForMode(scaler, mode);
            }
        }

        private static void ApplyScalerForCurrentOrientation(CanvasScaler scaler)
        {
            bool isPortrait = Screen.height > Screen.width;
            ApplyScalerForMode(scaler, isPortrait ? RcScreenMode.Portrait : RcScreenMode.Landscape);
        }

        private static void ApplyScalerForMode(CanvasScaler scaler, RcScreenMode mode)
        {
            if (mode == RcScreenMode.Portrait)
            {
                // 세로: 너비 기준으로 스케일 → 캔버스 너비 = 1080 고정
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }
            else
            {
                // 가로: 높이 기준으로 스케일 → 캔버스 높이 = 1080 고정
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1f;
            }
        }

        public void Dispose()
        {
            RcScreenOrientationApplier.OnModeApplied -= OnOrientationChanged;
            scalers.Clear();

            foreach (var kvp in canvases)
            {
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value.gameObject);
            }

            canvases.Clear();
        }
    }
}
