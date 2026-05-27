using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

/// <summary>
/// 레지스트리 한 행: SkinType → 해당 스킨 테이블 매핑.
/// </summary>
[Serializable]
public struct RcFaceSkinEntry
{
    [RcColumn(100f)] public uint                Id;
    [RcColumn(120f)] public RcFaceSkinType      SkinType;
    [RcColumn(220f)] public RcFaceSkinDataTable Table;
}

/// <summary>
/// SkinType → RcFaceSkinDataTable 매핑 레지스트리.
/// 스킨 추가 시 이 SO에 행 하나만 추가하면 됨.
/// </summary>
[CreateAssetMenu(fileName = "NewFaceSkinRegistry", menuName = "Rolice/Face Skin Registry")]
public class RcFaceSkinRegistry : RcDataTableSO<RcFaceSkinEntry>
{
    private Dictionary<RcFaceSkinType, RcFaceSkinDataTable> lookup;
    private Dictionary<uint, RcFaceSkinDataTable>           lookupById;

    protected override void OnTableChanged()
    {
        lookup     = new Dictionary<RcFaceSkinType, RcFaceSkinDataTable>();
        lookupById = new Dictionary<uint, RcFaceSkinDataTable>();
        if (Rows == null) return;
        foreach (var entry in Rows)
        {
            if (entry.Table == null) continue;
            lookup[entry.SkinType] = entry.Table;
            lookupById[entry.Id]   = entry.Table;
        }
    }

    /// <summary>스킨 타입으로 데이터 테이블 조회.</summary>
    public RcFaceSkinDataTable GetFaceData(RcFaceSkinType skinType)
    {
        if (lookup == null) OnTableChanged();
        if (lookup.TryGetValue(skinType, out var table)) return table;
        Debug.LogWarning($"[RcFaceSkinRegistry] SkinType not found: {skinType}");
        return null;
    }

    /// <summary>ItemId(uint)로 데이터 테이블 조회.</summary>
    public RcFaceSkinDataTable GetByItemId(uint itemId)
    {
        if (lookupById == null) OnTableChanged();
        if (lookupById.TryGetValue(itemId, out var table)) return table;
        Debug.LogWarning($"[RcFaceSkinRegistry] ItemId not found: {itemId}");
        return null;
    }
}
