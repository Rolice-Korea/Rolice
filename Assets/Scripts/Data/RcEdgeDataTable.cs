using System.Collections.Generic;
using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewEdgeDataTable", menuName = "Rolice/Edge Data Table")]
public class RcEdgeDataTable : ScriptableObject
{
    public RcEdgeData GetEdgeData(RcEdgeSkinType skinType)
    {
        if (edgeDatas != null && edgeDatas.TryGetValue(skinType, out var edgeData))
        {
            return edgeData;
        }

        Debug.LogWarning($"[RcEdgeDataTable] EdgeData not found for SkinType: {skinType}");
        return new RcEdgeData();
    }

    public Dictionary<RcEdgeSkinType, RcEdgeData> edgeDatas;
}
