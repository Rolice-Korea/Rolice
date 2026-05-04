using System.Collections.Generic;
using Engine.UI;
using Rolice;
using Rolice.System;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI의 비즈니스 로직을 담당하는 프레젠터.
    /// Shop 데이터 테이블에서 아이템을 가져와 뷰를 갱신하고, 구매 처리를 수행함.
    /// 
    /// Bag과의 핵심 차이:
    ///   - Bag: RcPlayerState(플레이어 보유 데이터)에서 아이템 목록 생성
    ///   - Shop: RcShopDataTable(상점 데이터 테이블)에서 판매 아이템 목록 생성
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType currentTab = RcShopTabType.Currency;
        private List<RcShopItemData> currentItems = new();
        private int selectedIndex = -1;

        protected override void OnInitialize()
        {
            // 이벤트 연결
            Panel.OnCloseClicked += HandleClose;
            Panel.OnBuyClicked += HandleBuy;
            Panel.OnTabChanged += HandleTabChanged;

            // 초기 뷰 설정
            RefreshView();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnBuyClicked -= HandleBuy;
            Panel.OnTabChanged -= HandleTabChanged;
        }

        private void RefreshView()
        {
            Panel.SelectTab((int)currentTab);
            RefreshItemList();
        }

        private void RefreshItemList()
        {
            var table = RcDataTableManager.ShopDataTable;
            if (table == null) return;

            currentItems = table.GetItemsByTab(currentTab);
            var playerState = RcPlayerState.Instance;

            Panel.RefreshItemList(currentItems.Count, (index, widget) =>
            {
                var itemData = currentItems[index];
                bool isOwned = playerState.HasOwnedItem(itemData.ItemId);
                string priceLabel = isOwned ? "보유" : $"{itemData.Price}";

                widget.Setup(index, itemData.PreviewColor, priceLabel, HandleItemSelected);
                widget.SetState(index == selectedIndex, isOwned);
            });

            UpdateSelectedDisplay();
        }

        private void UpdateSelectedDisplay()
        {
            if (Panel == null) return;

            if (selectedIndex >= 0 && selectedIndex < currentItems.Count)
            {
                var item = currentItems[selectedIndex];
                bool isOwned = RcPlayerState.Instance.HasOwnedItem(item.ItemId);

                Panel.SetSelectedItemName(item.DisplayName);
                Panel.SetSelectedItemPrice(isOwned ? "보유 중" : $"{item.Price} {item.CurrencyKey}");
                Panel.SetBuyButtonInteractable(!isOwned);
            }
            else
            {
                Panel.SetSelectedItemName("");
                Panel.SetSelectedItemPrice("");
                Panel.SetBuyButtonInteractable(false);
            }
        }

        private void HandleTabChanged(int index)
        {
            var nextTab = (RcShopTabType)index;
            if (currentTab == nextTab) return;

            currentTab = nextTab;
            selectedIndex = -1;
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            selectedIndex = index;
            UpdateSelectedDisplay();
            RefreshItemList();
        }

        private void HandleBuy()
        {
            if (selectedIndex < 0 || selectedIndex >= currentItems.Count) return;

            var item = currentItems[selectedIndex];
            var playerState = RcPlayerState.Instance;

            // 이미 보유한 아이템은 구매 불가
            if (playerState.HasOwnedItem(item.ItemId))
            {
                Debug.Log($"[RcUIShop] 이미 보유한 아이템: {item.ItemId}");
                return;
            }

            // TODO: 서버 연동 — 구매 요청을 서버로 보내고 응답 후 처리
            // 현재는 로컬 처리만 수행 (서버 미구현)
            
            Debug.Log($"[RcUIShop] 구매 요청: {item.ItemId}, 가격: {item.Price} {item.CurrencyKey}");

            // --- 로컬 처리 (서버 연동 시 아래 로직을 서버 응답 콜백으로 이동) ---
            // int currentCurrency = playerState.GetCurrency(item.CurrencyKey);
            // if (currentCurrency < item.Price)
            // {
            //     Debug.Log($"[RcUIShop] 재화 부족: {item.CurrencyKey} 현재={currentCurrency}, 필요={item.Price}");
            //     return;
            // }
            // playerState.SetCurrency(item.CurrencyKey, currentCurrency - item.Price);
            // playerState.AddOwnedItem(item.ItemId);
            // --- 로컬 처리 끝 ---

            RefreshItemList();
        }

        private void HandleClose()
        {
            Panel.Close();
        }
    }
}
