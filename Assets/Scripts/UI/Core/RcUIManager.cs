using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.UI
{
    public class RcUIManager : RcSingletonMono<RcUIManager>
    {
        [SerializeField, Tooltip("패널 루트 Transform. 비어있으면 자기 자신 사용.")]
        private Transform _panelRoot;

        private readonly Dictionary<Type, RcUIPanel> _panelCache = new();
        private readonly List<RcUIPanel> _panelStack = new();

        private void Awake()
        {
            InitializeSingleton();
            if (Instance != this) return;

            CachePanels();
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
                Instance.Toggle<RcTestPanel>();
        }

        // panelRoot 하위의 모든 RcUIPanel을 타입별로 캐시
        private void CachePanels()
        {
            var root = _panelRoot != null ? _panelRoot : transform;
            var panels = root.GetComponentsInChildren<RcUIPanel>(true);

            foreach (var panel in panels)
            {
                var type = panel.GetType();

                if (!_panelCache.TryAdd(type, panel))
                {
                    Debug.LogWarning($"[RcUIManager] 중복 패널 타입: {type.Name}", panel);
                    continue;
                }

                panel.gameObject.SetActive(false);
            }
        }

        public T Open<T>() where T : RcUIPanel
        {
            var panel = GetPanel<T>();
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
            var panel = GetPanel<T>();
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
            var panel = GetPanel<T>();
            if (panel == null || !panel.IsOpen) return;

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
            var panel = GetPanel<T>();
            if (panel == null) return;

            if (panel.IsOpen)
                Close<T>();
            else
                Open<T>();
        }

        public bool IsOpen<T>() where T : RcUIPanel
        {
            var panel = GetPanel<T>();
            return panel != null && panel.IsOpen;
        }

        public RcUIPanel CurrentPanel =>
            _panelStack.Count > 0 ? _panelStack[^1] : null;

        public int OpenPanelCount => _panelStack.Count;

        private T GetPanel<T>() where T : RcUIPanel
        {
            if (_panelCache.TryGetValue(typeof(T), out var panel))
                return panel as T;

            Debug.LogError($"[RcUIManager] 패널을 찾을 수 없음: {typeof(T).Name}");
            return null;
        }

        private RcUIPanel PopStack()
        {
            var last = _panelStack[^1];
            _panelStack.RemoveAt(_panelStack.Count - 1);
            return last;
        }
    }
}
