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
    [RcColumn(100f)] public string              Id;
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

    protected override void OnTableChanged()
    {
        lookup = new Dictionary<RcFaceSkinType, RcFaceSkinDataTable>();
        if (Rows == null) return;
        foreach (var entry in Rows)
            if (entry.Table != null)
                lookup[entry.SkinType] = entry.Table;
    }

    /// <summary>
    /// 지정된 스킨 타입에 매핑된 데이터 테이블(클래스)을 곧바로 반환합니다.
    /// </summary>
    public RcFaceSkinDataTable GetFaceData(RcFaceSkinType skinType)
    {
        if (lookup == null) OnTableChanged();
        if (lookup.TryGetValue(skinType, out var table))
        {
            return table;
        }

        Debug.LogWarning($"[RcFaceSkinRegistry] SkinType not found: {skinType}");
        return null;
    }
}
