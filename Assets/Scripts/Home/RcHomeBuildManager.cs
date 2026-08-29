using System;
using System.Collections.Generic;
using Engine;
using Rolice.Data;
using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 홈(마을 꾸미기)의 도메인 레이어 — 섬에 배치된 블록의 단일 진실(source of truth).
    /// Vector3Int 그리드(3D 복셀)로 배치/제거/조회를 관리하고, 변경을 이벤트로 알린다.
    /// 표현(블록 프리팹·커서·카메라)은 이벤트를 구독해 반응하며, 여기서는 transform을 다루지 않는다.
    ///
    /// 영속성과의 분리: 세이브 싱글톤(RcPlayerState)을 직접 참조하지 않고
    /// Load/WriteTo(RcHomeData) 시접만 노출한다(테스트 용이, 배선은 부트스트랩 담당).
    /// RcLevelManager와 동일하게 순수 C# 싱글톤(RcSingleton)으로 구현.
    /// </summary>
    public class RcHomeBuildManager : RcSingleton<RcHomeBuildManager>
    {
        // 런타임 그리드 모델. 좌표 → 블록. O(1) 조회.
        private readonly Dictionary<Vector3Int, RcPlacedBlock> blocks = new();

        // Load로 물린 세이브 DTO. 배치/제거마다 즉시 반영해 DTO를 항상 정합 상태로 유지한다.
        private RcHomeData attachedData;

        /// <summary>블록이 새로 배치됨(Load 시 기존 블록 복원에도 발행).</summary>
        public event Action<RcPlacedBlock> OnBlockPlaced;

        /// <summary>해당 좌표의 블록이 제거됨.</summary>
        public event Action<Vector3Int> OnBlockRemoved;

        /// <summary>
        /// 재고 규칙 주입점(DIP). 부트스트랩이 배선한다.
        /// null이면 재고 제약 없음 — 인벤토리를 배선하지 않은 개발용 씬에서 배치가 막히지 않게 하기 위함.
        /// 프로덕션 경로(로비/홈 부트스트랩)는 반드시 주입할 것.
        /// </summary>
        public IRcBlockInventory Inventory { get; set; }

        public int Count => blocks.Count;

        public bool HasBlock(Vector3Int position) => blocks.ContainsKey(position);

        public bool TryGetBlock(Vector3Int position, out RcPlacedBlock block)
            => blocks.TryGetValue(position, out block);

        /// <summary>
        /// 블록 배치. 좌표가 점유됐거나 재고가 없으면 실패(false). 성공 시 OnBlockPlaced 발행.
        /// (인접/지지 같은 배치 규칙은 인터랙션 레이어에서 점진 확장 — 모델은 임의 3D 좌표 허용.)
        ///
        /// 재고 검사를 여기 두는 이유: 배치 경로가 늘어도(UI 드래그, 자동 배치 등)
        /// 규칙이 새지 않는 단일 초크포인트가 되게 하기 위함.
        /// </summary>
        public bool TryPlace(Vector3Int position, int blockId, string skinId)
        {
            if (blocks.ContainsKey(position))
                return false;

            if (Inventory != null && !Inventory.CanPlace(blockId))
                return false;

            var block = new RcPlacedBlock(position, blockId, skinId);
            blocks.Add(position, block);
            Inventory?.OnPlaced(blockId);
            SyncToData();
            OnBlockPlaced?.Invoke(block);
            return true;
        }

        /// <summary>블록 제거. 해당 좌표가 비어 있으면 false. 성공 시 재고로 반환 후 OnBlockRemoved 발행.</summary>
        public bool Remove(Vector3Int position)
        {
            if (!blocks.TryGetValue(position, out var block))
                return false;

            blocks.Remove(position);
            Inventory?.OnRemoved(block.BlockId);
            SyncToData();
            OnBlockRemoved?.Invoke(position);
            return true;
        }

        /// <summary>
        /// 세이브 DTO로부터 그리드 모델을 재구성한다. 기존 상태는 비우고,
        /// 각 블록마다 OnBlockPlaced를 발행해 표현 레이어가 균일하게 스폰하도록 한다.
        /// (구독은 Load 호출 전에 끝나 있어야 함 — 부트스트랩 책임.)
        ///
        /// 재고 미차감: 저장된 개수는 이미 배치분이 빠진 잔여값이므로, 복원은 소비가 아니다.
        /// TryPlace를 우회해 직접 채우는 것이 이 불변량을 지키는 방식.
        /// </summary>
        public void Load(RcHomeData data)
        {
            Clear();
            attachedData = data;

            if (data?.Blocks == null)
                return;

            foreach (var block in data.Blocks)
            {
                // 손상/중복 좌표 방어: 첫 항목만 채택.
                if (blocks.ContainsKey(block.Position))
                    continue;

                blocks.Add(block.Position, block);
                OnBlockPlaced?.Invoke(block);
            }
        }

        /// <summary>현재 그리드 모델을 세이브 DTO에 반영한다(저장 직전 호출).</summary>
        public void WriteTo(RcHomeData data)
        {
            if (data == null)
                return;

            data.Blocks.Clear();
            foreach (var block in blocks.Values)
                data.Blocks.Add(block);
        }

        /// <summary>모델을 비우고 DTO 연결을 끊는다(이벤트 미발행 — 씬 종료/리셋용).</summary>
        public void Clear()
        {
            blocks.Clear();
            attachedData = null;
        }

        /// <summary>
        /// 배치/제거를 DTO에 즉시 반영한다.
        ///
        /// 인벤토리 개수는 DTO에 즉시 쓰이는데 블록 목록만 저장 시점까지 미루면,
        /// 그 사이 다른 시스템(상점 구매 등)이 SaveLocal을 호출할 때
        /// <b>개수는 깎였는데 블록은 없는</b> 세이브가 기록되어 블록이 증발한다.
        /// 결정 #9(편집 이탈 시 일괄 저장)는 디스크 기록 시점에 대한 것이고,
        /// 메모리상 DTO는 항상 정합이어야 한다.
        ///
        /// 비용: 클릭당 O(n) 리스트 재작성. n은 수백 수준이고 매 프레임이 아니라 무시 가능.
        /// </summary>
        private void SyncToData()
        {
            if (attachedData != null)
                WriteTo(attachedData);
        }
    }
}
