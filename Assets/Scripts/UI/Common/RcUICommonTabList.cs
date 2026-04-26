using System;
using System.Collections.Generic;
using Engine.UI;
using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 언리얼 엔진의 CommonTabListWidget 패턴을 참고한 범용 탭 리스트 관리자.
    /// 여러 개의 RcUICommonTabButton과 1:1로 대응되는 콘텐츠 패널들을 제어함.
    /// </summary>
    public class RcUICommonTabList : RcUIWidget
    {
        [Serializable]
        public class TabEntry
        {
            public string label;                 // 탭 표시 이름
            public RcUICommonTabButton tabButton; // 연결된 탭 버튼 슬롯
            public GameObject targetContent;     // 탭 선택 시 활성화될 콘텐츠 패널
            public Transform itemListParent;     // 해당 탭의 아이템들이 담길 부모 (추가됨)
        }

        [Header("Tab Configuration")]
        [SerializeField] private List<TabEntry> tabEntries = new();
        [SerializeField] private int defaultTabIndex = 0;

        public event Action<int> OnTabChanged;
        public int ActiveTabIndex { get; private set; } = -1;

        public Transform ActiveItemListParent
        {
            get
            {
                if (ActiveTabIndex >= 0 && ActiveTabIndex < tabEntries.Count)
                    return tabEntries[ActiveTabIndex].itemListParent;
                return null;
            }
        }

        public override void Initialize()
        {
            for (int i = 0; i < tabEntries.Count; i++)
            {
                var entry = tabEntries[i];
                if (entry.tabButton == null) continue;

                // 탭 버튼 초기화 및 데이터 주입
                entry.tabButton.Initialize();
                entry.tabButton.Setup(i, entry.label, HandleTabButtonClicked);
            }

            // 기본 탭 선택
            if (tabEntries.Count > 0)
            {
                SelectTab(defaultTabIndex);
            }
        }

        public override void Cleanup()
        {
            foreach (var entry in tabEntries)
            {
                if (entry.tabButton != null)
                    entry.tabButton.Cleanup();
            }
            
            OnTabChanged = null;
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabEntries.Count) return;
            if (ActiveTabIndex == index) return;

            ActiveTabIndex = index;

            // 모든 탭과 콘텐츠 상태 갱신
            for (int i = 0; i < tabEntries.Count; i++)
            {
                bool isSelected = (i == index);
                var entry = tabEntries[i];

                if (entry.tabButton != null)
                    entry.tabButton.SetSelected(isSelected);

                if (entry.targetContent != null)
                    entry.targetContent.SetActive(isSelected);
            }

            OnTabChanged?.Invoke(index);
        }

        private void HandleTabButtonClicked(int index)
        {
            SelectTab(index);
        }
        
        // 특정 인덱스의 탭 이름을 변경하고 싶을 때 사용
        public void UpdateTabLabel(int index, string newLabel)
        {
            if (index >= 0 && index < tabEntries.Count)
            {
                tabEntries[index].label = newLabel;
                // 만약 라이브 중에 변경된다면 탭 버튼의 Setup을 다시 호출하거나 직접 접근 필요
            }
        }
    }
}
