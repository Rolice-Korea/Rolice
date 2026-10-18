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
    private Dictionary<uint, RcEdgeSkinRow>           edgeDatasById;

    protected override void OnTableChanged()
    {
        edgeDatas     = new Dictionary<RcEdgeSkinType, RcEdgeSkinRow>();
        edgeDatasById = new Dictionary<uint, RcEdgeSkinRow>();
        if (Rows == null) return;
        foreach (var row in Rows)
        {
            edgeDatas[row.SkinType] = row;
            edgeDatasById[row.Id]   = row;
        }
    }

    /// <summary>스킨 타입으로 조회.</summary>
    public RcEdgeSkinRow GetEdgeData(RcEdgeSkinType skinType)
    {
        if (edgeDatas == null) OnTableChanged();
        if (edgeDatas.TryGetValue(skinType, out var edgeData)) return edgeData;
        Debug.LogWarning($"[RcEdgeSkinDataTable] EdgeData not found for SkinType: {skinType}");
        return new RcEdgeSkinRow();
    }

    /// <summary>경고 없이 스킨 타입으로 조회. 폴백을 직접 처리할 호출부용.</summary>
    public bool TryGetEdgeData(RcEdgeSkinType skinType, out RcEdgeSkinRow edgeData)
    {
        if (edgeDatas == null) OnTableChanged();
        return edgeDatas.TryGetValue(skinType, out edgeData);
    }

    /// <summary>ItemId(uint)로 조회.</summary>
    public RcEdgeSkinRow? GetByItemId(uint itemId)
    {
        if (edgeDatasById == null) OnTableChanged();
        if (edgeDatasById.TryGetValue(itemId, out var row)) return row;
        Debug.LogWarning($"[RcEdgeSkinDataTable] ItemId not found: {itemId}");
        return null;
    }
}
