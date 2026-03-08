using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewFaceDataTable", menuName = "Rolice/Face Data Table")]
public class RcFaceDataTable : ScriptableObject
{
    public RcFaceData GetFaceData(RcFaceSkinType skinType)
    {
        if (faceDatas != null && faceDatas.TryGetValue(skinType, out var faceData))
        {
            return faceData;
        }

        Debug.LogWarning($"[RcFaceDataTable] FaceData not found for SkinType: {skinType}");
        return new RcFaceData();
    }

    public Dictionary<RcFaceSkinType, RcFaceData> faceDatas;
}
