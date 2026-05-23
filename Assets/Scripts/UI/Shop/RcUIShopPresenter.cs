using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Engine.UI;
using Rolice;
using Rolice.System;
using Rolice.System.Backend;
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
        private bool isBuying;

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

        private void HandleBuy() => HandleBuyAsync().Forget();

        private async UniTaskVoid HandleBuyAsync()
        {
            if (isBuying) return;
            if (selectedIndex < 0 || selectedIndex >= currentItems.Count) return;

            var item = currentItems[selectedIndex];

            // 이미 보유한 아이템은 구매 불가 (이중 클릭 방어)
            if (RcPlayerState.Instance.HasOwnedItem(item.ItemId)) return;

            isBuying = true;
            Panel.SetBuyButtonInteractable(false);

            var economy = RcBackendServices.Economy;

            // 재화 차감 (Firestore 트랜잭션, 네트워크 오류 시 재시도 다이얼로그)
            bool success = false;
            await RcSystemDialogManager.Instance.ShowUntilSuccessAsync(async () =>
            {
                success = await economy.SpendAsync(item.CurrencyKey, item.Price);
            }, "Connection failed.\nPlease retry.");

            if (!success)
            {
                // TODO: 재화 부족 전용 팝업 (RcGemShopPanel 등 연계)
                Debug.Log($"[RcUIShop] 재화 부족: {item.CurrencyKey} 필요={item.Price}");
                isBuying = false;
                UpdateSelectedDisplay();
                return;
            }

            // 아이템 지급 (Firestore merge, 네트워크 오류 시 재시도 다이얼로그)
            // SpendAsync 성공 후 AddItemAsync 실패 시 재화만 차감되는 문제를 막기 위해 반드시 성공까지 대기
            await RcSystemDialogManager.Instance.ShowUntilSuccessAsync(
                () => economy.AddItemAsync(item.ItemId),
                "Connection failed.\nPlease retry.");

            Debug.Log($"[RcUIShop] 구매 완료: {item.ItemId}");

            isBuying = false;
            RefreshItemList();
        }

        private void HandleClose()
        {
            Panel.Close();
        }
    }
}
