using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RcCurrencyRow
{
    [RcColumn(160f)] public string ItemId;
    // TODO: Icon — 출처 결정 후 연결
}

/// <summary>
/// Currency 탭에 표시할 아이템 목록 테이블. (ID 범위: 10000~19999)
/// 가격 정보는 ItemId로 ShopDataTable에서 조회.
/// </summary>
[CreateAssetMenu(fileName = "NewCurrencyDataTable", menuName = "Rolice/Currency Data Table")]
public class RcCurrencyDataTable : RcDataTableSO<RcCurrencyRow>
{
    private Dictionary<string, RcCurrencyRow> byId;

    protected override void OnTableChanged() => Rebuild();

    private void Rebuild()
    {
        byId = new Dictionary<string, RcCurrencyRow>();
        if (Rows == null) return;

        foreach (var row in Rows)
            if (!string.IsNullOrEmpty(row.ItemId))
                byId[row.ItemId] = row;
    }

    public RcCurrencyRow? GetItem(string itemId)
    {
        if (byId == null) Rebuild();
        return byId.TryGetValue(itemId, out var row) ? row : null;
    }
}
