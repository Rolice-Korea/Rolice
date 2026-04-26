using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Engine.UI
{
    public class RcUIManager : RcSingletonMono<RcUIManager>
    {
        [Header("Registry")]
        [SerializeField, Tooltip("패널 레지스트리 SO")]
        private RcUIPanelRegistry registry;

        private RcUICanvasLayer canvasLayer;
        private readonly Dictionary<Type, RcUIPanel> instanceCache = new();
        private readonly List<RcUIPanel> panelStack = new();

        private void Awake()
        {
            InitializeSingleton();
            if (Instance != this) return;

            Initialize();
        }

        private void OnDestroy()
        {
            canvasLayer?.Dispose();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Initialize()
        {
            if (registry == null)
            {
                Debug.LogError("[RcUIManager] PanelRegistry가 할당되지 않았습니다.");
                return;
            }

            registry.Initialize();
            canvasLayer = new RcUICanvasLayer(transform, Camera.main);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(UpdateCameraDelayed());
        }

        private IEnumerator UpdateCameraDelayed()
        {
            yield return null;
            canvasLayer.UpdateCamera(Camera.main);
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

            panelStack.Add(panel);
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

            panelStack.Add(panel);
            panel.Open(data);
            return panel;
        }

        public void CloseCurrent(Action onComplete = null)
        {
            if (panelStack.Count == 0) return;

            var panel = PopStack();
            panel.Close(onComplete);
        }

        public void Close<T>(Action onComplete = null) where T : RcUIPanel
        {
            if (!instanceCache.TryGetValue(typeof(T), out var panel)) return;
            if (!panel.IsOpen) return;

            panelStack.Remove(panel);
            panel.Close(onComplete);
        }

        public void CloseAll()
        {
            for (int i = panelStack.Count - 1; i >= 0; i--)
            {
                var panel = panelStack[i];
                if (panel.IsOpen)
                    panel.CloseAnimated();
            }

            panelStack.Clear();
        }

        public void DeactivateAll()
        {
            foreach (var panel in instanceCache.Values)
            {
                if (panel.gameObject.activeSelf)
                    panel.Deactivate();
            }
        }

        public void SetCanvasVisible(bool visible)
        {
            canvasLayer.SetVisible(visible);
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

        public void OpenBagPanel()
        {
            // 명시적인 네임스페이스 사용
            this.Open<Rolice.UI.RcUIBagPanel>();
        }

        public bool IsOpen<T>() where T : RcUIPanel
        {
            if (!instanceCache.TryGetValue(typeof(T), out var panel)) return false;
            return panel.IsOpen;
        }

        public RcUIPanel CurrentPanel =>
            panelStack.Count > 0 ? panelStack[^1] : null;

        public int OpenPanelCount => panelStack.Count;

        private T GetOrCreatePanel<T>() where T : RcUIPanel
        {
            var type = typeof(T);

            if (instanceCache.TryGetValue(type, out var cached))
                return cached as T;

            if (!registry.TryGetEntry<T>(out var entry))
            {
                Debug.LogError($"[RcUIManager] 레지스트리에 없는 패널: {type.Name}");
                return null;
            }

            var root = canvasLayer.GetRoot(entry.Layer);
            var instance = Instantiate(entry.Prefab, root);
            instance.gameObject.SetActive(false);

            var panel = instance as T;
            instanceCache[type] = panel;

            return panel;
        }

        private RcUIPanel PopStack()
        {
            var last = panelStack[^1];
            panelStack.RemoveAt(panelStack.Count - 1);
            return last;
        }
    }
}
