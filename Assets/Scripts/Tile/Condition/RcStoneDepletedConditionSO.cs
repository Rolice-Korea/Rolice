using UnityEngine;

/// 조건: 스톤이 모두 소진되었는지 판단
[CreateAssetMenu(fileName = "StoneDepletedCondition", menuName = "Rolice/Tile/Condition/Stone Depleted")]
public class RcStoneDepletedConditionSO : RcTileConditionSO
{
    [Tooltip("최대 히트 횟수")]
    public int MaxStoneCount = 3;

    public override bool Evaluate(RcDicePawn pawn, RcTileData tileData)
    {
        if (tileData is not RcStoneTileData stoneTile) return false;
        return stoneTile.HitCount >= MaxStoneCount;
    }
}
