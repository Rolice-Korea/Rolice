using System;
using System.Collections.Generic;
using UnityEngine;
using Rolice;

[Serializable]
public struct RcShopItemData
{
    public string ItemId;
    public string DisplayName;
    public RcShopTabType TabType;
    public int Price;
    public string CurrencyKey;
    public Color PreviewColor;
}

[CreateAssetMenu(fileName = "NewShopDataTable", menuName = "Rolice/Shop Data Table")]
public class RcShopDataTable : ScriptableObject
{
    [SerializeField] private RcShopItemData[] shopItems;

    private Dictionary<RcShopTabType, List<RcShopItemData>> itemsByTab;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        itemsByTab = new Dictionary<RcShopTabType, List<RcShopItemData>>();

        if (shopItems == null) return;

        foreach (var item in shopItems)
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
        if (itemsByTab == null) BuildLookup();

        if (itemsByTab.TryGetValue(tabType, out var list))
            return list;

        return new List<RcShopItemData>();
    }

    public RcShopItemData? GetItemById(string itemId)
    {
        if (shopItems == null) return null;

        foreach (var item in shopItems)
        {
            if (item.ItemId == itemId)
                return item;
        }

        return null;
    }
}
