using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcEdgeSkinRow
{
    [RcColumn(100f)] public uint            Id;
    [RcColumn(120f)] public RcEdgeSkinType SkinType;
    [RcColumn(200f)] public Material       EdgeMaterial;
    [RcColumn(150f)] public Sprite         IconSprite;
}

[CreateAssetMenu(fileName = "NewEdgeSkinDataTable", menuName = "Rolice/Edge Skin Data Table")]
public class RcEdgeSkinDataTable : RcDataTableSO<RcEdgeSkinRow>
{
    private Dictionary<RcEdgeSkinType, RcEdgeSkinRow> edgeDatas;

    protected override void OnTableChanged()
    {
        edgeDatas = new Dictionary<RcEdgeSkinType, RcEdgeSkinRow>();
        if (Rows == null) return;
        foreach (var row in Rows)
        {
            edgeDatas[row.SkinType] = row;
        }
    }

    public RcEdgeSkinRow GetEdgeData(RcEdgeSkinType skinType)
    {
        if (edgeDatas == null) OnTableChanged();
        if (edgeDatas.TryGetValue(skinType, out var edgeData))
            return edgeData;

        Debug.LogWarning($"[RcEdgeSkinDataTable] EdgeData not found for SkinType: {skinType}");
        return new RcEdgeSkinRow();
    }
}
