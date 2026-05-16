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
        }

        protected override void OnDispose()
        {
            Panel.OnCloseClicked -= HandleClose;
            Panel.OnApplyClicked -= HandleApply;
            Panel.OnResetClicked -= HandleReset;
            Panel.OnTabChanged -= HandleTabChanged;
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
            var table = RcDataTableManager.FaceSkinRegistry;
            if (table == null) return;

            // TODO: 실제 데이터 테이블에서 리스트를 가져오는 기능을 보강해야 할 수도 있음
            // 현재는 Enum을 기반으로 루프 (데이터 테이블 확장에 따라 변경 가능)
            int count = (int)RcFaceSkinType.Max;
            Panel.RefreshItemList(count, (index, widget) =>
            {
                var type = (RcFaceSkinType)index;
                var skinData = table.GetFaceData(type);
                
                // RcFaceData에는 Name 필드가 없으므로 Enum 타입명을 기본 이름으로 사용
                string label = type.ToString();
                
                // 프리뷰 색상 (필요 시 데이터에서 추출)
                Color previewColor = Color.white;
                if (skinData.HasValue)
                {
                    var colorMat = skinData.Value.GetFaceMaterial(RcColorType.White);
                    if (colorMat != null) previewColor = colorMat.color;
                }

                widget.Setup(index, previewColor, HandleItemSelected);
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
            var table = RcDataTableManager.EdgeDataTable;
            if (table == null) return;

            int count = (int)RcEdgeSkinType.Max;
            Panel.RefreshItemList(count, (index, widget) =>
            {
                var type = (RcEdgeSkinType)index;
                widget.Setup(index, Color.gray, HandleItemSelected);
                widget.SetState(selectedEdgeSkin == type, false);
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
                selectedFaceSkin = (RcFaceSkinType)index;
            else
                selectedEdgeSkin = (RcEdgeSkinType)index;

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

            Debug.Log($"[RcUIBag] Skins Applied: Face={activeFaceSkin}, Edge={activeEdgeSkin}");
            
            // 전역 스킨 갱신 (이미 게임 중에 오픈되었을 경우 현재 객체들에 반영을 위해 주사위 비주얼 갱신 등을 호출할 수 있음)
            // 여기서는 단순히 패널 닫기 처리를 할 수도 있고, 메시지를 띄울 수도 있음
        }

        private void HandleReset()
        {
            // 현재 저장된 상태로 되돌림
            selectedFaceSkin = activeFaceSkin;
            selectedEdgeSkin = activeEdgeSkin;
            RefreshView();
        }

        private void HandleClose()
        {
            Panel.Close();
        }
    }
}
