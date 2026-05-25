using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RcShopRow
{
    [RcColumn(160f)] public string     ItemId;
    [RcColumn(100f)] public RcCostType CostType;
    [RcColumn(80f)]  public int        CostValue;
}

/// <summary>
/// 아이템 가격 조회 테이블. ItemId → CostType / CostValue.
/// 탭별 아이템 목록은 각 DataTable(Currency/FaceSkin/EdgeSkin)이 주도.
/// ItemId는 IEconomyService.HasItem / AddItemAsync에 그대로 사용하는 string key.
/// </summary>
[CreateAssetMenu(fileName = "NewShopDataTable", menuName = "Rolice/Shop Data Table")]
public class RcShopDataTable : RcDataTableSO<RcShopRow>
{
    private Dictionary<string, RcShopRow> byId;

    protected override void OnTableChanged() => Rebuild();

    private void Rebuild()
    {
        byId = new Dictionary<string, RcShopRow>();
        if (Rows == null) return;

        foreach (var row in Rows)
            if (!string.IsNullOrEmpty(row.ItemId))
                byId[row.ItemId] = row;
    }

    /// <summary>ItemId로 가격 정보 조회. 없으면 null.</summary>
    public RcShopRow? GetItem(string itemId)
    {
        if (byId == null) Rebuild();
        return byId.TryGetValue(itemId, out var row) ? row : null;
    }
}
