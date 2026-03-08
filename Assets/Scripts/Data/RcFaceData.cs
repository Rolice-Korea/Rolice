using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcFaceData
{
    public Material GetFaceMaterial(RcColorType colorType)
    {
        if (faceMaterials != null && faceMaterials.TryGetValue(colorType, out var faceMaterial))
        {
            return faceMaterial;
        }

        Debug.LogWarning($"[RcFaceData] FaceMaterial not found for ColorType: {colorType}");
        return null;
    }

    public GameObject GetMatchEffect(RcColorType colorType)
    {
        if (matchEffects != null && matchEffects.TryGetValue(colorType, out var matchEffect))
        {
            return matchEffect;
        }

        Debug.LogWarning($"[RcFaceData] MatchEffect not found for ColorType: {colorType}");
        return null;
    }

    public RcFaceSkinType skinType;
    public Dictionary<RcColorType, Material> faceMaterials;
    public Dictionary<RcColorType, GameObject> matchEffects;
}
