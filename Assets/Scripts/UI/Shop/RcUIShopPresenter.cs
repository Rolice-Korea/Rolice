using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI 프레젠터.
    /// ShopDataTable이 카탈로그 주도권을 가지며, 비주얼은 각 DataTable에서 역조회.
    /// 구매 효과는 IItemEffect.ApplyAsync()에 위임.
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType    currentTab    = RcShopTabType.Currency;
        private int              selectedIndex = -1;
        private RcShopRow?       selectedRow;

        // 현재 탭 index → ShopRow 매핑 (Refresh 시 재구성)
        private readonly List<RcShopRow> currentRows = new();

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleClose;
            Panel.OnBuyClicked   += HandleBuy;
            Panel.OnTabChanged   += HandleTabChanged;
            RefreshView();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnBuyClicked   -= HandleBuy;
            Panel.OnTabChanged   -= HandleTabChanged;
        }

        // ─── Refresh ────────────────────────────────────────────────────────

        private void RefreshView()
        {
            Panel.SelectTab((int)currentTab);
            currentRows.Clear();

            var itemType = TabToItemType(currentTab);
            var rows     = RcDataTableManager.ShopDataTable?.GetItemsByType(itemType);

            if (rows == null || rows.Count == 0)
            {
                Panel.RefreshItemList((int)currentTab, 0, null);
                UpdateSelectedDisplay();
                return;
            }

            foreach (var r in rows) currentRows.Add(r);

            Panel.RefreshItemList((int)currentTab, currentRows.Count, (index, widget) =>
            {
                var    row     = currentRows[index];
                bool   isOwned = IsOwned(row);
                string price   = FormatPrice(row);
                var    visual  = GetVisual(row);

                widget.Setup(index, visual.color, visual.icon, price, HandleItemSelected);
                widget.SetState(index == selectedIndex, isOwned);
            });

            UpdateSelectedDisplay();
        }

        // ─── Visual ─────────────────────────────────────────────────────────

        private (Color color, Sprite icon) GetVisual(RcShopRow row)
        {
            switch (row.ItemType)
            {
                case RcItemType.FaceSkin:
                {
                    var table = RcDataTableManager.FaceSkinRegistry?.GetByItemId(row.ItemId);
                    var icon  = table != null ? table.IconSprite : null;
                    var mat   = table?.GetFaceMaterial(RcColorType.White);
                    var color = mat != null ? mat.color : Color.white;
                    return (color, icon);
                }
                case RcItemType.EdgeSkin:
                {
                    var rowData = RcDataTableManager.EdgeSkinDataTable?.GetByItemId(row.ItemId);
                    var icon    = rowData.HasValue ? rowData.Value.IconSprite : null;
                    var color   = rowData.HasValue && rowData.Value.EdgeMaterial != null
                        ? rowData.Value.EdgeMaterial.color
                        : Color.gray;
                    return (color, icon);
                }
                default:
                    return (Color.white, null);
            }
        }

        // ─── Selected Display ────────────────────────────────────────────────

        private void UpdateSelectedDisplay()
        {
            if (Panel == null) return;

            if (selectedIndex < 0 || !selectedRow.HasValue)
            {
                Panel.SetSelectedItemName("");
                Panel.SetSelectedItemPrice("");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            var row = selectedRow.Value;
            Panel.SetSelectedItemName(row.ItemId.ToString());

            if (IsOwned(row))
            {
                Panel.SetSelectedItemPrice("보유 중");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            Panel.SetSelectedItemPrice(FormatPrice(row));
            Panel.SetBuyButtonInteractable(row.Effect != null);
        }

        // ─── Handlers ───────────────────────────────────────────────────────

        private void HandleTabChanged(int index)
        {
            var next = (RcShopTabType)index;
            if (currentTab == next) return;
            currentTab    = next;
            selectedIndex = -1;
            selectedRow   = null;
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            selectedIndex = index;
            selectedRow   = (index >= 0 && index < currentRows.Count)
                ? currentRows[index]
                : (RcShopRow?)null;
            RefreshView();
        }

        private void HandleBuy()  => HandleBuyAsync().Forget();

        private async UniTaskVoid HandleBuyAsync()
        {
            if (!selectedRow.HasValue || selectedRow.Value.Effect == null)
            {
                Debug.LogWarning("[RcUIShop] 선택된 아이템 없음 또는 Effect 미설정");
                return;
            }

            var row = selectedRow.Value;

            Panel.SetBuyButtonInteractable(false);
            try
            {
                bool success = await RcBackendServices.Economy
                    .SpendAsync(row.CostType.ToKey(), row.CostValue);

                if (!success)
                {
                    Debug.Log($"[RcUIShop] 잔액 부족: {row.CostType} {row.CostValue}");
                    Panel.SetSelectedItemPrice("잔액 부족");
                    return;
                }

                await row.Effect.ApplyAsync(row.ItemId);
                Debug.Log($"[RcUIShop] 구매 완료: {row.ItemId}");
                RefreshView();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RcUIShop] 구매 실패: {e.Message}");
                Panel.SetBuyButtonInteractable(true);
            }
        }

        private void HandleClose() => Panel.Close();

        // ─── Helpers ────────────────────────────────────────────────────────

        private static RcItemType TabToItemType(RcShopTabType tab) => tab switch
        {
            RcShopTabType.Currency => RcItemType.Currency,
            RcShopTabType.Face     => RcItemType.FaceSkin,
            RcShopTabType.Edge     => RcItemType.EdgeSkin,
            _                      => RcItemType.Currency,
        };

        private static bool IsOwned(RcShopRow row)
            => row.Effect != null
            && !row.Effect.IsRepurchasable
            && RcBackendServices.Economy.HasItem(row.ItemId.ToString());

        private static string FormatPrice(RcShopRow row)
        {
            string symbol = row.CostType == RcCostType.Gold ? "⭐" : "💎";
            return $"{row.CostValue} {symbol}";
        }
    }
}
