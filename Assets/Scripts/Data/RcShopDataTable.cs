using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcShopItemData
{
    [RcColumn(120f)] public string        ItemId;
    [RcColumn(120f)] public string        DisplayName;
    [RcColumn( 80f)] public RcShopTabType TabType;
    [RcColumn( 60f)] public int           Price;
    [RcColumn(100f)] public string        CurrencyKey;
    [RcColumn( 80f)] public Color         PreviewColor;
}

[CreateAssetMenu(fileName = "NewShopDataTable", menuName = "Rolice/Shop Data Table")]
public class RcShopDataTable : RcDataTableSO<RcShopItemData>
{
    private Dictionary<RcShopTabType, List<RcShopItemData>> itemsByTab;

    protected override void OnTableChanged()
    {
        itemsByTab = new Dictionary<RcShopTabType, List<RcShopItemData>>();
        if (Rows == null) return;
        foreach (var item in Rows)
        {
            if (!itemsByTab.TryGetValue(item.TabType, out var list))
            {
                list = new List<RcShopItemData>();
                itemsByTab[item.TabType] = list;
            }
            list.Add(item);
        }
    }

    public List<RcShopItemData> GetItemsByTab(RcShopTabType tabType)
    {
        if (itemsByTab == null) OnTableChanged();
        if (itemsByTab.TryGetValue(tabType, out var list))
            return list;
        return new List<RcShopItemData>();
    }

    public RcShopItemData? GetItemById(string itemId)
    {
        if (Rows == null) return null;
        foreach (var item in Rows)
            if (item.ItemId == itemId) return item;
        return null;
    }
}
