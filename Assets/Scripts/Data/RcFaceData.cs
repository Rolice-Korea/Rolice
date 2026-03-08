using System;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcColorMaterialEntry
{
    public RcColorType ColorType;
    public Material    Material;
}

[Serializable]
public struct RcColorEffectEntry
{
    public RcColorType ColorType;
    public GameObject  Effect;
}

[Serializable]
public struct RcColorBundle
{
    public RcColorType  ColorType;
    public Material     FaceMaterial;
    public Material     TileMaterial;
    public GameObject   MatchEffect;
}

[Serializable]
public struct RcSkinBundle
{
    public RcFaceSkinType  SkinType;
    public RcColorBundle[] Colors;
}

[Serializable]
public struct RcFaceData
{
    public RcFaceSkinType SkinType;
    [SerializeField] private RcColorMaterialEntry[] materialEntries;
    [SerializeField] private RcColorMaterialEntry[] tileMaterialEntries;
    [SerializeField] private RcColorEffectEntry[]   effectEntries;

    public static RcFaceData Build(RcFaceSkinType skin, RcColorBundle[] bundles)
    {
        var data = new RcFaceData { SkinType = skin };
        if (bundles == null) return data;

        data.materialEntries     = new RcColorMaterialEntry[bundles.Length];
        data.tileMaterialEntries = new RcColorMaterialEntry[bundles.Length];
        data.effectEntries       = new RcColorEffectEntry[bundles.Length];

        for (int i = 0; i < bundles.Length; i++)
        {
            var b = bundles[i];
            data.materialEntries[i]     = new RcColorMaterialEntry { ColorType = b.ColorType, Material = b.FaceMaterial };
            data.tileMaterialEntries[i] = new RcColorMaterialEntry { ColorType = b.ColorType, Material = b.TileMaterial };
            data.effectEntries[i]       = new RcColorEffectEntry   { ColorType = b.ColorType, Effect   = b.MatchEffect  };
        }
        return data;
    }

    public Material GetFaceMaterial(RcColorType colorType)
    {
        if (materialEntries == null) return null;
        foreach (var entry in materialEntries)
            if (entry.ColorType == colorType) return entry.Material;
        Debug.LogWarning($"[RcFaceData] FaceMaterial not found for ColorType: {colorType}");
        return null;
    }

    public Material GetTileMaterial(RcColorType colorType)
    {
        if (tileMaterialEntries == null) return null;
        foreach (var entry in tileMaterialEntries)
            if (entry.ColorType == colorType) return entry.Material;
        Debug.LogWarning($"[RcFaceData] TileMaterial not found for ColorType: {colorType}");
        return null;
    }

    public GameObject GetMatchEffect(RcColorType colorType)
    {
        if (effectEntries == null) return null;
        foreach (var entry in effectEntries)
            if (entry.ColorType == colorType) return entry.Effect;
        Debug.LogWarning($"[RcFaceData] MatchEffect not found for ColorType: {colorType}");
        return null;
    }
}
