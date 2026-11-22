using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice;
using Rolice.Define;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 메인 상점 UI 프레젠터 (Face/Edge 코스메틱 전용).
    /// 별(Star) 시스템 제거에 따라 모든 아이템은 해금 게이트 없이 결제 비용(Free 또는 Gem)만으로 구매한다.
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType currentTab    = RcShopTabType.Face;
        private int           selectedIndex = -1;
        private RcShopRow?    selectedRow;

        private readonly List<RcShopRow> currentRows = new();

        private struct ItemVisual
        {
            public Color  Color;
            public Sprite Icon;
            public string Name;
        }

        protected override void OnInitialize()
        {
            Panel.OnCloseClicked += HandleClose;
            Panel.OnBuyClicked   += HandleBuy;
            Panel.OnTabChanged   += HandleTabChanged;
            RefreshView();

            RcLobbyCube.Instance?.RestoreActiveSkin();
            RcLobbyCube.Instance?.SlideToPreview();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnBuyClicked   -= HandleBuy;
            Panel.OnTabChanged   -= HandleTabChanged;

            RcLobbyCube.Instance?.RestoreActiveSkin();
            RcLobbyCube.Instance?.SlideToCenter();
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
                var row    = currentRows[index];
                var visual = GetVisual(row);

                widget.Setup(index, visual.Color, visual.Icon, FormatCost(row), HandleItemSelected);
                widget.SetState(ResolveState(row, index));
            });

            UpdateSelectedDisplay();
        }

        /// <summary>선택만 바뀐 경우 — Setup 재호출 없이 슬롯 상태만 경량 갱신.</summary>
        private void RefreshSelectionStates()
        {
            Panel.RefreshItemStates((int)currentTab, (index, widget) =>
            {
                if (index < 0 || index >= currentRows.Count) return;
                widget.SetState(ResolveState(currentRows[index], index));
            });
            UpdateSelectedDisplay();
        }

        // ─── State / Visual ─────────────────────────────────────────────────

        private RcShopItemState ResolveState(RcShopRow row, int index)
        {
            if (index == selectedIndex) return RcShopItemState.Selected;
            if (IsOwned(row))           return RcShopItemState.Owned;
            return RcShopItemState.Normal;
        }

        private ItemVisual GetVisual(RcShopRow row)
        {
            switch (row.ItemType)
            {
                case RcItemType.FaceSkin:
                {
                    var table = RcDataTableManager.FaceSkinRegistry?.GetByItemId(row.ItemId);
                    var icon  = table != null ? table.IconSprite : null;
                    var color = RcSkinPreviewColor.FromFaceSkin(table, Color.white);
                    var name  = table != null ? table.SkinType.ToString() : row.ItemId.ToString();
                    return new ItemVisual { Color = color, Icon = icon, Name = name };
                }
                case RcItemType.EdgeSkin:
                {
                    var rowData = RcDataTableManager.EdgeSkinDataTable?.GetByItemId(row.ItemId);
                    var icon    = rowData.HasValue ? rowData.Value.IconSprite : null;
                    var edgeMat = rowData.HasValue ? rowData.Value.EdgeMaterial : null;
                    var color   = RcSkinPreviewColor.FromEdgeMaterial(edgeMat, Color.gray);
                    var name    = rowData.HasValue ? rowData.Value.SkinType.ToString() : row.ItemId.ToString();
                    return new ItemVisual { Color = color, Icon = icon, Name = name };
                }
                default:
                    return new ItemVisual { Color = Color.white, Icon = null, Name = row.ItemId.ToString() };
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
            Panel.SetSelectedItemName(GetVisual(row).Name);

            if (IsOwned(row))
            {
                Panel.SetSelectedItemPrice("보유 중");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            Panel.SetSelectedItemPrice(FormatCost(row));
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

            RcLobbyCube.Instance?.RestoreActiveSkin();
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            selectedIndex = index;
            selectedRow   = (index >= 0 && index < currentRows.Count)
                ? currentRows[index]
                : (RcShopRow?)null;

            if (selectedRow.HasValue)
                PreviewItem(selectedRow.Value);

            RefreshSelectionStates();
        }

        private static void PreviewItem(RcShopRow row)
        {
            var cube = RcLobbyCube.Instance;
            if (cube == null) return;

            switch (row.ItemType)
            {
                case RcItemType.FaceSkin:
                {
                    var table = RcDataTableManager.FaceSkinRegistry?.GetByItemId(row.ItemId);
                    if (table != null) cube.PreviewFaceSkin(table);
                    break;
                }
                case RcItemType.EdgeSkin:
                {
                    var rowData = RcDataTableManager.EdgeSkinDataTable?.GetByItemId(row.ItemId);
                    if (rowData.HasValue) cube.PreviewEdgeSkin(rowData.Value.SkinType);
                    break;
                }
            }
        }

        private void HandleBuy() => HandleBuyAsync().Forget();

        private async UniTaskVoid HandleBuyAsync()
        {
            if (!selectedRow.HasValue || selectedRow.Value.Effect == null)
            {
                Debug.LogWarning("[RcUIShop] 선택된 아이템 없음 또는 Effect 미설정");
                return;
            }

            var row = selectedRow.Value;

            if (IsOwned(row)) return;

            Panel.SetBuyButtonInteractable(false);
            try
            {
                if (row.CostType == RcShopCostType.Gem)
                {
                    bool success = await RcBackendServices.Economy
                        .SpendAsync(RcCurrencyId.Gem, row.CostValue);

                    if (!success)
                    {
                        Debug.Log($"[RcUIShop] 잔액 부족: Gem {row.CostValue}");
                        Panel.SetSelectedItemPrice("잔액 부족");
                        Panel.SetBuyButtonInteractable(true);
                        return;
                    }
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
            RcShopTabType.Face => RcItemType.FaceSkin,
            RcShopTabType.Edge => RcItemType.EdgeSkin,
            _                  => RcItemType.FaceSkin,
        };

        private static bool IsOwned(RcShopRow row)
            => row.Effect != null
            && !row.Effect.IsRepurchasable
            && RcBackendServices.Economy.HasItem(row.ItemId.ToString());

        private static string FormatCost(RcShopRow row)
            => row.CostType == RcShopCostType.Free
                ? "무료"
                : $"{row.CostValue} 💎";
    }
}
