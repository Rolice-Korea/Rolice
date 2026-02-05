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

        [SerializeField] private List<PanelEntry> _entries = new();

        private Dictionary<Type, PanelEntry> _cache;

        /// <summary>
        /// 런타임 초기화. 타입별로 캐싱.
        /// </summary>
        public void Initialize()
        {
            _cache = new Dictionary<Type, PanelEntry>();

            foreach (var entry in _entries)
            {
                if (entry.Prefab == null)
                {
                    Debug.LogWarning("[RcUIPanelRegistry] null 프리팹 엔트리 발견, 건너뜀.");
                    continue;
                }

                var type = entry.Prefab.GetType();

                if (!_cache.TryAdd(type, entry))
                {
                    Debug.LogWarning($"[RcUIPanelRegistry] 중복 패널 타입: {type.Name}");
                }
            }
        }

        /// <summary>
        /// 타입으로 패널 엔트리 조회.
        /// </summary>
        public bool TryGetEntry<T>(out PanelEntry entry) where T : RcUIPanel
        {
            if (_cache == null) Initialize();
            return _cache.TryGetValue(typeof(T), out entry);
        }

        /// <summary>
        /// 타입으로 패널 엔트리 조회 (non-generic).
        /// </summary>
        public bool TryGetEntry(Type panelType, out PanelEntry entry)
        {
            if (_cache == null) Initialize();
            return _cache.TryGetValue(panelType, out entry);
        }
    }
}
