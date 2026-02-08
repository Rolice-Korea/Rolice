using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    public class RcUICanvasLayer
    {
        private readonly Transform _root;
        private readonly Camera _uiCamera;
        private readonly Dictionary<RcUILayer, Canvas> _canvases = new();

        public RcUICanvasLayer(Transform root, Camera uiCamera = null)
        {
            _root = root;
            _uiCamera = uiCamera;
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
            if (_canvases.TryGetValue(layer, out var existing))
                return existing;

            var go = new GameObject($"Canvas_{layer}");
            go.transform.SetParent(_root, false);

            var canvas = go.AddComponent<Canvas>();

            if (_uiCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = _uiCamera;
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

            _canvases[layer] = canvas;
            return canvas;
        }

        public void Dispose()
        {
            foreach (var kvp in _canvases)
            {
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value.gameObject);
            }

            _canvases.Clear();
        }
    }
}
