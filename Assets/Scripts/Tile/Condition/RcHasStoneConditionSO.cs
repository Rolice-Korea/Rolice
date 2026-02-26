using UnityEngine;

/// 조건: 스톤이 아직 남아있는지 판단 (아직 부술 수 있음)
[CreateAssetMenu(fileName = "HasStoneCondition", menuName = "Rolice/Tile/Condition/Has Stone")]
public class RcHasStoneConditionSO : RcTileConditionSO
{
    [Tooltip("최대 히트 횟수")]
    public int MaxStoneCount = 3;

    public override bool Evaluate(RcDicePawn pawn, RcTileData tileData)
    {
        if (tileData is not RcStoneTileData stoneTile) return false;
        return stoneTile.HitCount < MaxStoneCount;
    }
}
