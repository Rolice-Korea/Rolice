using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialDatabase", menuName = "Rolice/Database/Material Database")]
public class RcMaterialDataBaseSO : ScriptableObject
{
    [SerializeField] private MaterialEntry[] materials;

    [System.Serializable]
    public class MaterialEntry
    {
        public Material Material;

        [Header("메타데이터")]
        public ColorType ColorType;
        public MaterialUsageType UsageType;

        public bool IsValid()
        {
            return Material != null;
        }
    }

    // === Runtime Cache ===
    private Dictionary<(ColorType, MaterialUsageType), Material> _cache;
    private Dictionary<Material, MaterialEntry> _reverseCache;

    public void Initialize()
    {
        _cache = new Dictionary<(ColorType, MaterialUsageType), Material>();
        _reverseCache = new Dictionary<Material, MaterialEntry>();

        foreach (var entry in materials)
        {
            if (entry == null || !entry.IsValid())
                continue;

            var key = (entry.ColorType, entry.UsageType);

            if (_cache.ContainsKey(key))
            {
                Debug.LogWarning($"[MaterialDatabase] Duplicate entry: {entry.ColorType} {entry.UsageType}");
                continue;
            }

            _cache[key] = entry.Material;
            _reverseCache[entry.Material] = entry;
        }

    }

    public Material GetMaterial(ColorType color, MaterialUsageType usage)
    {
        if (_cache == null) Initialize();

        return _cache != null && _cache.TryGetValue((color, usage), out Material mat) ? mat : null;
    }

    public MaterialEntry GetEntry(ColorType color, MaterialUsageType usage)
    {
        if (_cache == null) Initialize();

        var mat = GetMaterial(color, usage);
        return mat != null ? GetEntryByMaterial(mat) : null;
    }

    public MaterialEntry GetEntryByMaterial(Material mat)
    {
        if (_reverseCache == null) Initialize();

        return _reverseCache != null && _reverseCache.TryGetValue(mat, out MaterialEntry entry) ? entry : null;
    }

    public ColorType? GetColorType(Material mat)
    {
        var entry = GetEntryByMaterial(mat);
        return entry?.ColorType;
    }

    public MaterialUsageType? GetUsageType(Material mat)
    {
        var entry = GetEntryByMaterial(mat);
        return entry?.UsageType;
    }

    public List<Material> GetMaterialsByUsage(MaterialUsageType usage)
    {
        if (_cache == null) Initialize();

        return materials
            .Where(e => e != null && e.IsValid() && e.UsageType == usage)
            .Select(e => e.Material)
            .ToList();
    }

    public bool HasMaterial(ColorType color, MaterialUsageType usage)
    {
        return GetMaterial(color, usage) != null;
    }

    public IReadOnlyList<MaterialEntry> AllMaterials => materials;
}

public enum MaterialUsageType
{
    Dice,           // 주사위 면
    Tile,           // 타일 (일반 상태)
    Effect,         // 이펙트/파티클
}