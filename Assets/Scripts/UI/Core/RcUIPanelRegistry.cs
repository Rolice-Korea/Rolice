using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.UI
{
    [CreateAssetMenu(fileName = "PanelRegistry", menuName = "Rolice/UI/Panel Registry")]
    public class RcUIPanelRegistry : ScriptableObject
    {
        [Serializable]
        public class PanelEntry
        {
            [Tooltip("패널 프리팹")]
            public RcUIPanel Prefab;

            [Tooltip("이 패널이 속할 UI 레이어")]
            public RcUILayer Layer;
        }

        [SerializeField] private List<PanelEntry> entries = new();

        private Dictionary<Type, PanelEntry> cache;

        public void Initialize()
        {
            cache = new Dictionary<Type, PanelEntry>();

            foreach (var entry in entries)
            {
                if (entry.Prefab == null)
                {
                    Debug.LogWarning("[RcUIPanelRegistry] null 프리팹 엔트리 발견, 건너뜀.");
                    continue;
                }

                var type = entry.Prefab.GetType();

                if (!cache.TryAdd(type, entry))
                {
                    Debug.LogWarning($"[RcUIPanelRegistry] 중복 패널 타입: {type.Name}");
                }
            }
        }

        public bool TryGetEntry<T>(out PanelEntry entry) where T : RcUIPanel
        {
            if (cache == null) Initialize();
            return cache.TryGetValue(typeof(T), out entry);
        }

        public bool TryGetEntry(Type panelType, out PanelEntry entry)
        {
            if (cache == null) Initialize();
            return cache.TryGetValue(panelType, out entry);
        }
    }
}
