using System;
using Engine.UI;
using Rolice;
using Rolice.System;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 상점 UI의 비즈니스 로직을 담당하는 프레젠터.
    /// 각 탭(Currency/Face/Edge)별로 해당 DataTable을 직접 참조하여 아이템 목록을 생성함.
    /// </summary>
    public class RcUIShopPresenter : RcUIPresenter<RcUIShopPanel>
    {
        private RcShopTabType currentTab = RcShopTabType.Currency;
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

            switch (currentTab)
            {
                case RcShopTabType.Currency:
                    RefreshCurrencyList();
                    break;
                case RcShopTabType.Face:
                    RefreshFaceList();
                    break;
                case RcShopTabType.Edge:
                    RefreshEdgeList();
                    break;
            }

            UpdateSelectedDisplay();
        }

        private void RefreshCurrencyList()
        {
            // TODO: Currency 탭 — 동료 작업 예정
            Panel.RefreshItemList((int)currentTab, 0, null);
        }

        private void RefreshFaceList()
        {
            var table = RcDataTableManager.FaceSkinRegistry;
            if (table == null) return;

            int count = (int)RcFaceSkinType.Max;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var type = (RcFaceSkinType)index;
                var skinData = table.GetFaceData(type);
                Sprite iconSprite = skinData != null ? skinData.IconSprite : null;

                Color previewColor = Color.white;
                if (skinData != null)
                {
                    var colorMat = skinData.GetFaceMaterial(RcColorType.White);
                    if (colorMat != null) previewColor = colorMat.color;
                }

                widget.Setup(index, previewColor, iconSprite, "", HandleItemSelected);
                widget.SetState(index == selectedIndex, false);
            });
        }

        private void RefreshEdgeList()
        {
            var table = RcDataTableManager.EdgeSkinDataTable;
            if (table == null || table.Rows == null) return;

            int count = table.Rows.Length;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var row = table.Rows[index];
                Sprite iconSprite = row.IconSprite;
                
                Color previewColor = Color.gray;
                if (row.EdgeMaterial != null)
                {
                    previewColor = row.EdgeMaterial.color;
                }

                widget.Setup(index, previewColor, iconSprite, "", HandleItemSelected);
                widget.SetState(index == selectedIndex, false);
            });
        }

        private void UpdateSelectedDisplay()
        {
            if (Panel == null) return;

            if (selectedIndex < 0)
            {
                Panel.SetSelectedItemName("");
                Panel.SetSelectedItemPrice("");
                Panel.SetBuyButtonInteractable(false);
                return;
            }

            string itemName = "";
            string itemPrice = ""; // 가격 정보는 나중에 표시

            if (currentTab == RcShopTabType.Face)
            {
                var type = (RcFaceSkinType)selectedIndex;
                itemName = type.ToString();
            }
            else if (currentTab == RcShopTabType.Edge)
            {
                var type = (RcEdgeSkinType)selectedIndex;
                itemName = type.ToString();
            }

            Panel.SetSelectedItemName(itemName);
            Panel.SetSelectedItemPrice(itemPrice);
            Panel.SetBuyButtonInteractable(true);
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
            RefreshView();
        }

        private void HandleBuy()
        {
            // TODO: 구매 로직 — 나중에 구현
            Debug.Log($"[RcUIShop] Buy requested: tab={currentTab}, index={selectedIndex}");
        }

        private void HandleClose()
        {
            Panel.Close();
        }
    }
}
