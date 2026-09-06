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
    ///
    /// 구매 판정은 두 축으로 분리된다:
    ///   - 해금 게이트  : 누적 별 합(RcProgressManager.GetTotalStars) ≥ row.RequiredStars. 별은 소모되지 않음.
    ///   - 결제 비용    : 해금된 뒤 Free(무상) 또는 Gem 소모.
    ///
    /// 잼 획득(광고/IAP)과 하트 충전은 별도 팝업(RcGemShopPanel / RcHeartShopPanel)이 담당한다.
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType    currentTab    = RcShopTabType.Face;
        private int              selectedIndex = -1;
        private RcShopRow?       selectedRow;

        // 현재 탭 index → ShopRow 매핑 (Refresh 시 재구성)
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

            // 로비 주사위 프리뷰 위치로 슬라이드
            RcLobbyCube.Instance?.SlideToPreview();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnBuyClicked   -= HandleBuy;
            Panel.OnTabChanged   -= HandleTabChanged;

            // 로비 주사위 원위치 복귀
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

            int totalStars = TotalStars;

            Panel.RefreshItemList((int)currentTab, currentRows.Count, (index, widget) =>
            {
                var row    = currentRows[index];
                var visual = GetVisual(row);

                widget.Setup(index, visual.Color, visual.Icon, FormatSlotLabel(row, totalStars), HandleItemSelected);
                widget.SetState(ResolveState(row, index, totalStars));
            });

            UpdateSelectedDisplay();
        }

        /// <summary>선택만 바뀐 경우 — Setup 재호출 없이 슬롯 상태만 경량 갱신.</summary>
        private void RefreshSelectionStates()
        {
            int totalStars = TotalStars;
            Panel.RefreshItemStates((int)currentTab, (index, widget) =>
            {
                if (index < 0 || index >= currentRows.Count) return;
                widget.SetState(ResolveState(currentRows[index], index, totalStars));
            });
            UpdateSelectedDisplay();
        }

        // ─── State / Visual ─────────────────────────────────────────────────

        private RcShopItemState ResolveState(RcShopRow row, int index, int totalStars)
        {
            if (index == selectedIndex)        return RcShopItemState.Selected;
            if (IsOwned(row))                  return RcShopItemState.Owned;
            if (totalStars < row.RequiredStars) return RcShopItemState.Locked;
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
                    var mat   = table?.GetFaceMaterial(RcColorType.White);
                    var color = mat != null && mat.HasColor("_BaseColor")
                        ? mat.GetColor("_BaseColor")
                        : Color.white;
                    var name  = table != null ? table.SkinType.ToString() : row.ItemId.ToString();
                    return new ItemVisual { Color = color, Icon = icon, Name = name };
                }
                case RcItemType.EdgeSkin:
                {
                    var rowData = RcDataTableManager.EdgeSkinDataTable?.GetByItemId(row.ItemId);
                    var icon    = rowData.HasValue ? rowData.Value.IconSprite : null;
                    var edgeMat = rowData.HasValue ? rowData.Value.EdgeMaterial : null;
                    var color   = edgeMat != null && edgeMat.HasColor("_BaseColor")
                        ? edgeMat.GetColor("_BaseColor")
                        : Color.gray;
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

            if (TotalStars < row.RequiredStars)
            {
                Panel.SetSelectedItemPrice($"★{row.RequiredStars} 필요");
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
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            selectedIndex = index;
            selectedRow   = (index >= 0 && index < currentRows.Count)
                ? currentRows[index]
                : (RcShopRow?)null;
            RefreshSelectionStates();
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

            if (IsOwned(row))                  return;
            if (TotalStars < row.RequiredStars) return;

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

        private static int TotalStars => RcProgressManager.Instance.GetTotalStars();

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

        /// <summary>슬롯 라벨: 잠금이면 해금 조건, 아니면 결제 비용.</summary>
        private static string FormatSlotLabel(RcShopRow row, int totalStars)
            => totalStars < row.RequiredStars
                ? $"★{row.RequiredStars}"
                : FormatCost(row);

        private static string FormatCost(RcShopRow row)
            => row.CostType == RcShopCostType.Free
                ? "무료"
                : $"{row.CostValue} 💎";
    }
}
