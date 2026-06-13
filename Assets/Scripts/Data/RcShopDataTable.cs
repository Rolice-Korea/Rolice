using System;
using System.Collections.Generic;
using Rolice;
using UnityEngine;

/// <summary>
/// 상점 카탈로그 한 행 (Face/Edge 코스메틱 전용).
///
/// 구매 가능 여부는 두 축으로 분리된다:
///   - RequiredStars : 누적 별 합이 이 값 이상이어야 구매 칸이 열리는 해금 게이트. 별은 소모되지 않음. 0이면 게이트 없음.
///   - CostType/Cost : 해금된 뒤 실제로 지불하는 비용. Free(무상) 또는 Gem(잼 소모).
///
/// 이 조합으로 "잼 전용", "별 해금 후 무상", "별 해금 후 잼 결제" 를 모두 표현한다.
/// Effect는 [SerializeReference]로 다형성 직렬화됨 — 행마다 다른 구체 타입 가능.
/// </summary>
[Serializable]
public struct RcShopRow
{
    [RcColumn(100f)] public uint           ItemId;
    [RcColumn(100f)] public RcItemType     ItemType;
    [RcColumn(100f)] public int            RequiredStars; // 누적 별 해금 게이트 (0 = 없음)
    [RcColumn(100f)] public RcShopCostType CostType;
    [RcColumn(80f)]  public int            CostValue;     // CostType == Gem 일 때 잼 수량
    [RcColumn(220f), SerializeReference] public IItemEffect Effect;
}

/// <summary>
/// 상점 카탈로그 테이블. ItemId → RcShopRow.
/// 탭별 목록의 주도권을 가지며, 비주얼 정보는 각 DataTable(FaceSkin/EdgeSkin)에서 조회.
/// </summary>
[CreateAssetMenu(fileName = "NewShopDataTable", menuName = "Rolice/Shop Data Table")]
public class RcShopDataTable : RcDataTableSO<RcShopRow>
{
    private Dictionary<uint, RcShopRow>                   byId;
    private Dictionary<RcItemType, List<RcShopRow>>       byType;

    protected override void OnTableChanged() => Rebuild();

    private void Rebuild()
    {
        byId   = new Dictionary<uint, RcShopRow>();
        byType = new Dictionary<RcItemType, List<RcShopRow>>();

        if (Rows == null) return;

        foreach (var row in Rows)
        {
            byId[row.ItemId] = row;

            if (!byType.TryGetValue(row.ItemType, out var list))
            {
                list = new List<RcShopRow>();
                byType[row.ItemType] = list;
            }
            list.Add(row);
        }
    }

    /// <summary>ItemId로 행 조회. 없으면 null.</summary>
    public RcShopRow? GetItem(uint itemId)
    {
        if (byId == null) Rebuild();
        return byId.TryGetValue(itemId, out var row) ? row : null;
    }

    /// <summary>탭 타입별 행 목록. 없으면 빈 배열.</summary>
    public IReadOnlyList<RcShopRow> GetItemsByType(RcItemType itemType)
    {
        if (byType == null) Rebuild();
        return byType.TryGetValue(itemType, out var list)
            ? list
            : Array.Empty<RcShopRow>();
    }
}
