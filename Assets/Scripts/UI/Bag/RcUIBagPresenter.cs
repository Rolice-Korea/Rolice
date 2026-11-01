using System.Collections.Generic;
using Engine.UI;
using Rolice;
using Rolice.System;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 가방 UI의 비즈니스 로직을 담당하는 프레젠터.
    /// 데이터(DataTable, PlayerState)와 뷰(RcUIBagPanel)를 연결함.
    /// </summary>
    public class RcUIBagPresenter : RcUIPresenter<RcUIBagPanel>
    {
        private RcBagTabType currentTab = RcBagTabType.Face;
        
        // 현재 적용된 스킨 (저장된 값)
        private RcFaceSkinType activeFaceSkin;
        private RcEdgeSkinType activeEdgeSkin;
        
        // 사용자가 현재 선택 중인 스킨 (임시 값)
        private RcFaceSkinType selectedFaceSkin;
        private RcEdgeSkinType selectedEdgeSkin;

        protected override void OnInitialize()
        {
            // 초기 데이터 로드
            var data = RcPlayerState.Instance.Data;
            activeFaceSkin = data.SelectedFaceSkin;
            activeEdgeSkin = data.SelectedEdgeSkin;
            
            // 임시 선택 상태 초기화
            selectedFaceSkin = activeFaceSkin;
            selectedEdgeSkin = activeEdgeSkin;

            // 이벤트 연결
            Panel.OnCloseClicked += HandleClose;
            Panel.OnApplyClicked += HandleApply;
            Panel.OnResetClicked += HandleReset;
            Panel.OnTabChanged += HandleTabChanged;

            // 초기 뷰 설정
            RefreshView();

            // 로비 주사위를 장착 스킨으로 맞춘 뒤 프리뷰 위치로 슬라이드
            RcLobbyCube.Instance?.RestoreActiveSkin();
            RcLobbyCube.Instance?.SlideToPreview();
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnApplyClicked -= HandleApply;
            Panel.OnResetClicked -= HandleReset;
            Panel.OnTabChanged -= HandleTabChanged;

            // 미적용 프리뷰 원복 후 로비 주사위 원위치 복귀
            RcLobbyCube.Instance?.RestoreActiveSkin();
            RcLobbyCube.Instance?.SlideToCenter();
        }

        private void RefreshView()
        {
            Panel.SelectTab((int)currentTab);
            
            if (currentTab == RcBagTabType.Face)
                RefreshFaceList();
            else
                RefreshEdgeList();
        }

        private void RefreshFaceList()
        {
            var registry = RcDataTableManager.FaceSkinRegistry;
            if (registry == null || registry.Rows == null) return;

            var rows = registry.Rows;
            int count = rows.Length;
            Panel.RefreshItemList((int)currentTab, count, (index, widget) =>
            {
                var entry = rows[index];
                var type = entry.SkinType;
                var skinData = entry.Table;
                Sprite iconSprite = skinData != null ? skinData.IconSprite : null;
                Color previewColor = RcSkinPreviewColor.FromFaceSkin(skinData, Color.white);

                widget.Setup(index, previewColor, iconSprite, HandleItemSelected);
                widget.SetState(selectedFaceSkin == type, false);
            });

            UpdateSelectedNameDisplay();
        }

        private void UpdateSelectedNameDisplay()
        {
            if (Panel == null) return;

            string skinName = currentTab == RcBagTabType.Face 
                ? selectedFaceSkin.ToString() 
                : selectedEdgeSkin.ToString();
            
            Panel.SetSelectedSkinName(skinName);
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
                Color previewColor = RcSkinPreviewColor.FromEdgeMaterial(row.EdgeMaterial, Color.gray);

                widget.Setup(index, previewColor, iconSprite, HandleItemSelected);
                widget.SetState(selectedEdgeSkin == row.SkinType, false);
            });

            UpdateSelectedNameDisplay();
        }

        private void HandleTabChanged(int index)
        {
            var nextTab = (RcBagTabType)index;
            if (currentTab == nextTab) return;

            currentTab = nextTab;
            RefreshView();
        }

        private void HandleItemSelected(int index)
        {
            if (currentTab == RcBagTabType.Face)
            {
                var rows = RcDataTableManager.FaceSkinRegistry?.Rows;
                if (rows == null || index < 0 || index >= rows.Length) return;

                selectedFaceSkin = rows[index].SkinType;
                RcLobbyCube.Instance?.PreviewFaceSkin(selectedFaceSkin);
            }
            else
            {
                // Edge 리스트는 table.Rows 순서로 그려지므로 index ≠ enum 값일 수 있음
                var rows = RcDataTableManager.EdgeSkinDataTable?.Rows;
                if (rows == null || index < 0 || index >= rows.Length) return;

                selectedEdgeSkin = rows[index].SkinType;
                RcLobbyCube.Instance?.PreviewEdgeSkin(selectedEdgeSkin);
            }

            UpdateSelectedNameDisplay();
            RefreshView();
        }

        private void HandleApply()
        {
            // 데이터 영속화
            var state = RcPlayerState.Instance;
            state.Data.SelectedFaceSkin = selectedFaceSkin;
            state.Data.SelectedEdgeSkin = selectedEdgeSkin;
            state.SaveLocal(); // 로컬 저장 호출

            activeFaceSkin = selectedFaceSkin;
            activeEdgeSkin = selectedEdgeSkin;

            // 저장된 값 기준으로 프리뷰 확정 (선택값과 동일하지만 단일 진실 원천을 따름)
            RcLobbyCube.Instance?.RestoreActiveSkin();

            Debug.Log($"[RcUIBag] Skins Applied: Face={activeFaceSkin}, Edge={activeEdgeSkin}");
        }

        private void HandleReset()
        {
            // 현재 저장된 상태로 되돌림
            selectedFaceSkin = activeFaceSkin;
            selectedEdgeSkin = activeEdgeSkin;
            RcLobbyCube.Instance?.RestoreActiveSkin();
            RefreshView();
        }

        private void HandleClose()
        {
            Panel.Close();
        }
    }
}
