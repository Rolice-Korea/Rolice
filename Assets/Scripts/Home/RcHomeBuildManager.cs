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

        /// <summary>블록이 새로 배치됨(Load 시 기존 블록 복원에도 발행).</summary>
        public event Action<RcPlacedBlock> OnBlockPlaced;

        /// <summary>해당 좌표의 블록이 제거됨.</summary>
        public event Action<Vector3Int> OnBlockRemoved;

        public int Count => blocks.Count;

        public bool HasBlock(Vector3Int position) => blocks.ContainsKey(position);

        public bool TryGetBlock(Vector3Int position, out RcPlacedBlock block)
            => blocks.TryGetValue(position, out block);

        /// <summary>
        /// 블록 배치. 좌표가 이미 점유됐으면 실패(false). 성공 시 OnBlockPlaced 발행.
        /// (인접/지지 같은 배치 규칙은 인터랙션 레이어에서 점진 확장 — 모델은 임의 3D 좌표 허용.)
        /// </summary>
        public bool TryPlace(Vector3Int position, int blockId, string skinId)
        {
            if (blocks.ContainsKey(position))
                return false;

            var block = new RcPlacedBlock(position, blockId, skinId);
            blocks.Add(position, block);
            OnBlockPlaced?.Invoke(block);
            return true;
        }

        /// <summary>블록 제거. 해당 좌표가 비어 있으면 false. 성공 시 OnBlockRemoved 발행.</summary>
        public bool Remove(Vector3Int position)
        {
            if (!blocks.Remove(position))
                return false;

            OnBlockRemoved?.Invoke(position);
            return true;
        }

        /// <summary>
        /// 세이브 DTO로부터 그리드 모델을 재구성한다. 기존 상태는 비우고,
        /// 각 블록마다 OnBlockPlaced를 발행해 표현 레이어가 균일하게 스폰하도록 한다.
        /// (구독은 Load 호출 전에 끝나 있어야 함 — 부트스트랩 책임.)
        /// </summary>
        public void Load(RcHomeData data)
        {
            Clear();

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

        /// <summary>모델을 비운다(이벤트 미발행 — 씬 종료/리셋용).</summary>
        public void Clear()
        {
            blocks.Clear();
        }
    }
}
