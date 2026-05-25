using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice;
using Rolice.System;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI의 비즈니스 로직을 담당하는 프레젠터.
    /// 각 탭(Currency/Face/Edge)별로 해당 DataTable을 직접 참조하여 아이템 목록을 생성함.
    /// 아이템 선택·구매는 string ItemId 기반으로 동작한다.
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType  currentTab    = RcShopTabType.Currency;
        private int            selectedIndex = -1;
        private string         selectedItemId;

        // 현재 탭 기준 index → itemId 매핑 (Refresh 시 재구성)
        private readonly List<string> currentItemIds = new();

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

            currentItemIds.Clear();

            switch (currentTab)
            {
                case RcShopTabType.Currency: RefreshCurrencyList(); break;
                case RcShopTabType.Face:     RefreshFaceList();     break;
                case RcShopTabType.Edge:     RefreshEdgeList();     break;
            }

            UpdateSelectedDisplay();
        }

        private void RefreshCurrencyList()
        {
            var currencyTable = RcDataTableManager.CurrencyDataTable;
            if (currencyTable == null || currencyTable.Rows == null)
            {
                Panel.RefreshItemList((int)currentTab, 0, null);
                return;
            }

            int count = currencyTable.Rows.Length;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var    row      = currencyTable.Rows[index];
                string itemId   = row.ItemId;
                currentItemIds.Add(itemId);

                var    shopRow  = RcDataTableManager.ShopDataTable?.GetItem(itemId);
                string price    = FormatPrice(shopRow);
                bool   isOwned  = RcBackendServices.Economy.HasItem(itemId);

                widget.Setup(index, Color.white, null, price, HandleItemSelected);
                widget.SetState(index == selectedIndex, isOwned);
            });
        }

        private void RefreshFaceList()
        {
            var registry = RcDataTableManager.FaceSkinRegistry;
            if (registry == null || registry.Rows == null) return;

            int count = registry.Rows.Length;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var    entry      = registry.Rows[index];
                string itemId     = entry.Id.ToString();
                currentItemIds.Add(itemId);

                var    skinData   = entry.Table;
                Sprite icon       = skinData != null ? skinData.IconSprite : null;
                Color  color      = Color.white;
                if (skinData != null)
                {
                    var mat = skinData.GetFaceMaterial(RcColorType.White);
                    if (mat != null) color = mat.color;
                }

                var    shopRow = RcDataTableManager.ShopDataTable?.GetItem(itemId);
                string price   = FormatPrice(shopRow);
                bool   isOwned = RcBackendServices.Economy.HasItem(itemId);

                widget.Setup(index, color, icon, price, HandleItemSelected);
                widget.SetState(index == selectedIndex, isOwned);
            });
        }

        private void RefreshEdgeList()
        {
            var table = RcDataTableManager.EdgeSkinDataTable;
            if (table == null || table.Rows == null) return;

            int count = table.Rows.Length;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var    row    = table.Rows[index];
                string itemId = row.Id.ToString();
                currentItemIds.Add(itemId);

                Sprite icon   = row.IconSprite;
                Color  color  = row.EdgeMaterial != null ? row.EdgeMaterial.color : Color.gray;

                var    shopRow = RcDataTableManager.ShopDataTable?.GetItem(itemId);
                string price   = FormatPrice(shopRow);
                bool   isOwned = RcBackendServices.Economy.HasItem(itemId);

                widget.Setup(index, color, icon, price, HandleItemSelected);
                widget.SetState(index == selectedIndex, isOwned);
            });
        }

        // ─── Selected Display ────────────────────────────────────────────────

        private void UpdateSelectedDisplay()
        {
            if (Panel == null) return;

            if (selectedIndex < 0 || string.IsNullOrEmpty(selectedItemId))
            {
                Panel.SetSelectedItemName("");
                Panel.SetSelectedItemPrice("");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            // 이미 보유한 아이템은 구매 버튼 비활성
            bool isOwned = RcBackendServices.Economy.HasItem(selectedItemId);
            if (isOwned)
            {
                Panel.SetSelectedItemName(selectedItemId);
                Panel.SetSelectedItemPrice("보유 중");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            var shopRow = RcDataTableManager.ShopDataTable?.GetItem(selectedItemId);
            Panel.SetSelectedItemName(selectedItemId);
            Panel.SetSelectedItemPrice(FormatPrice(shopRow));
            Panel.SetBuyButtonInteractable(shopRow.HasValue);
        }

        // ─── Handlers ───────────────────────────────────────────────────────

        private void HandleTabChanged(int index)
        {
            var nextTab = (RcShopTabType)index;
            if (currentTab == nextTab) return;

            currentTab     = nextTab;
            selectedIndex  = -1;
            selectedItemId = null;
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            selectedIndex  = index;
            selectedItemId = (index >= 0 && index < currentItemIds.Count)
                ? currentItemIds[index]
                : null;
            RefreshView();
        }

        private void HandleBuy() => HandleBuyAsync().Forget();

        private async UniTaskVoid HandleBuyAsync()
        {
            if (string.IsNullOrEmpty(selectedItemId))
            {
                Debug.LogWarning("[RcUIShop] 선택된 아이템 없음");
                return;
            }

            var shopRow = RcDataTableManager.ShopDataTable?.GetItem(selectedItemId);
            if (!shopRow.HasValue)
            {
                Debug.LogWarning($"[RcUIShop] ShopDataTable에 없는 아이템: {selectedItemId}");
                return;
            }

            string currencyKey = shopRow.Value.CostType.ToKey();
            int    cost        = shopRow.Value.CostValue;

            Panel.SetBuyButtonInteractable(false);
            try
            {
                bool success = await RcBackendServices.Economy.SpendAsync(currencyKey, cost);
                if (!success)
                {
                    Debug.Log($"[RcUIShop] 잔액 부족: {currencyKey} {cost}");
                    Panel.SetSelectedItemPrice("잔액 부족");
                    return;
                }

                await RcBackendServices.Economy.AddItemAsync(selectedItemId);
                Debug.Log($"[RcUIShop] 구매 완료: {selectedItemId}");

                // 구매 후 목록 갱신 (Owned 상태 반영)
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

        private static string FormatPrice(RcShopRow? shopRow)
        {
            if (!shopRow.HasValue) return "";
            string symbol = shopRow.Value.CostType == RcCostType.Gold ? "⭐" : "💎";
            return $"{shopRow.Value.CostValue} {symbol}";
        }
    }
}
