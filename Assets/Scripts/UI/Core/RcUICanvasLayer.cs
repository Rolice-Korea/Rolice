using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    public class RcUICanvasLayer
    {
        private readonly Transform root;
        private Camera camera;
        private readonly Dictionary<RcUILayer, Canvas> canvases = new();

        public RcUICanvasLayer(Transform root, Camera camera = null)
        {
            this.root = root;
            this.camera = camera;
        }

        public void UpdateCamera(Camera camera)
        {
            this.camera = camera;
            foreach (var kvp in canvases)
            {
                if (kvp.Value == null) continue;

                if (camera != null)
                {
                    kvp.Value.renderMode = RenderMode.ScreenSpaceCamera;
                    kvp.Value.worldCamera = camera;
                }
                else
                {
                    kvp.Value.renderMode = RenderMode.ScreenSpaceOverlay;
                }
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

            if (camera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 0f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            canvas.sortingOrder = (int)layer;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<CanvasGroup>();

            canvases[layer] = canvas;
            return canvas;
        }

        public void Dispose()
        {
            foreach (var kvp in canvases)
            {
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value.gameObject);
            }

            canvases.Clear();
        }
    }
}
