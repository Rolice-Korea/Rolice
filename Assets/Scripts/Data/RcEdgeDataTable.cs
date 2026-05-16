using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcEdgeData
{
    [RcColumn(120f)] public RcEdgeSkinType SkinType;
    [RcColumn(200f)] public Material       EdgeMaterial;
}

[CreateAssetMenu(fileName = "NewEdgeDataTable", menuName = "Rolice/Edge Data Table")]
public class RcEdgeDataTable : RcDataTableSO<RcEdgeData>
{
    private Dictionary<RcEdgeSkinType, RcEdgeData> edgeDatas;

    protected override void OnTableChanged() =>
        edgeDatas = BuildLookup(r => r.SkinType);

    public RcEdgeData GetEdgeData(RcEdgeSkinType skinType)
    {
        if (edgeDatas == null) OnTableChanged();
        if (edgeDatas.TryGetValue(skinType, out var edgeData))
            return edgeData;

        Debug.LogWarning($"[RcEdgeDataTable] EdgeData not found for SkinType: {skinType}");
        return new RcEdgeData();
    }
}
