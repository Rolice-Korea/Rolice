using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rolice.Data
{
    /// <summary>
    /// 섬에 배치된 블록 1개의 저장 레코드.
    /// 식별 축 분리: BlockId = 홈 도메인(사이즈/형태, 상점 독립), SkinId = 기존 상점 itemId.
    /// </summary>
    [Serializable]
    public struct RcPlacedBlock
    {
        public Vector3Int Position;
        public int        BlockId;
        public string     SkinId;

        public RcPlacedBlock(Vector3Int position, int blockId, string skinId)
        {
            Position = position;
            BlockId  = blockId;
            SkinId   = skinId;
        }
    }

    /// <summary>
    /// 홈(마을 꾸미기) 저장 컨테이너. RcPlayerData에 집약되어 기존 세이브/클라우드 싱크에 탑승.
    /// 런타임 그리드 모델(Vector3Int 딕셔너리)은 RcHomeBuildManager가 소유하고,
    /// 여기는 직렬화 가능한 순수 DTO만 보관한다(JsonUtility: List는 OK, Dictionary는 불가).
    /// </summary>
    [Serializable]
    public class RcHomeData
    {
        public List<RcPlacedBlock> Blocks = new();
    }
}
