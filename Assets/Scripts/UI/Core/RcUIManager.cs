using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.UI
{
    public class RcUIManager : RcSingletonMono<RcUIManager>
    {
        [Header("Registry")]
        [SerializeField, Tooltip("패널 레지스트리 SO")]
        private RcUIPanelRegistry _registry;

        [Header("Canvas Settings")]
        [SerializeField, Tooltip("UI 카메라 (null이면 ScreenSpace-Overlay)")]
        private Camera _uiCamera;

        private RcUICanvasLayer _canvasLayer;
        private readonly Dictionary<Type, RcUIPanel> _instanceCache = new();
        private readonly List<RcUIPanel> _panelStack = new();

        private void Awake()
        {
            InitializeSingleton();
            if (Instance != this) return;

            Initialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Instance.Toggle<RcTestPanel>();
            }
        }

        private void OnDestroy()
        {
            _canvasLayer?.Dispose();
        }

        private void Initialize()
        {
            if (_registry == null)
            {
                Debug.LogError("[RcUIManager] PanelRegistry가 할당되지 않았습니다.");
                return;
            }

            _registry.Initialize();
            _canvasLayer = new RcUICanvasLayer(transform, _uiCamera);
        }

        public T Open<T>() where T : RcUIPanel
        {
            var panel = GetOrCreatePanel<T>();
            if (panel == null) return null;

            if (panel.IsOpen)
            {
                Debug.LogWarning($"[RcUIManager] {typeof(T).Name} 이미 열려있음.");
                return panel;
            }

            _panelStack.Add(panel);
            panel.Open();
            return panel;
        }

        public T Open<T, TData>(TData data) where T : RcUIPanel<TData>
        {
            var panel = GetOrCreatePanel<T>();
            if (panel == null) return null;

            if (panel.IsOpen)
            {
                Debug.LogWarning($"[RcUIManager] {typeof(T).Name} 이미 열려있음.");
                return panel;
            }

            _panelStack.Add(panel);
            panel.Open(data);
            return panel;
        }

        public void CloseCurrent(Action onComplete = null)
        {
            if (_panelStack.Count == 0) return;

            var panel = PopStack();
            panel.Close(onComplete);
        }

        public void Close<T>(Action onComplete = null) where T : RcUIPanel
        {
            if (!_instanceCache.TryGetValue(typeof(T), out var panel)) return;
            if (!panel.IsOpen) return;

            _panelStack.Remove(panel);
            panel.Close(onComplete);
        }

        public void CloseAll()
        {
            for (int i = _panelStack.Count - 1; i >= 0; i--)
            {
                var panel = _panelStack[i];
                if (panel.IsOpen)
                    panel.Close();
            }

            _panelStack.Clear();
        }

        public void Toggle<T>() where T : RcUIPanel
        {
            var panel = GetOrCreatePanel<T>();
            if (panel == null) return;

            if (panel.IsOpen)
                Close<T>();
            else
                Open<T>();
        }

        public bool IsOpen<T>() where T : RcUIPanel
        {
            if (!_instanceCache.TryGetValue(typeof(T), out var panel)) return false;
            return panel.IsOpen;
        }

        public RcUIPanel CurrentPanel =>
            _panelStack.Count > 0 ? _panelStack[^1] : null;

        public int OpenPanelCount => _panelStack.Count;

        private T GetOrCreatePanel<T>() where T : RcUIPanel
        {
            var type = typeof(T);

            if (_instanceCache.TryGetValue(type, out var cached))
                return cached as T;

            if (!_registry.TryGetEntry<T>(out var entry))
            {
                Debug.LogError($"[RcUIManager] 레지스트리에 없는 패널: {type.Name}");
                return null;
            }

            var root = _canvasLayer.GetRoot(entry.Layer);
            var instance = Instantiate(entry.Prefab, root);
            instance.gameObject.SetActive(false);

            var panel = instance as T;
            _instanceCache[type] = panel;

            return panel;
        }

        private RcUIPanel PopStack()
        {
            var last = _panelStack[^1];
            _panelStack.RemoveAt(_panelStack.Count - 1);
            return last;
        }
    }
}
