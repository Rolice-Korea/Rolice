using UnityEngine;

/// 조건: 주사위 바닥 색상과 타일 색상이 일치하는지 판단
[CreateAssetMenu(fileName = "ColorMatchCondition", menuName = "Rolice/Tile/Condition/Color Match")]
public class RcColorMatchConditionSO : RcTileConditionSO
{
    public override bool Evaluate(RcDicePawn pawn, RcTileData tileData)
    {
        if (pawn == null || tileData == null) return false;

        RcColorSO tileColor = tileData.Color;
        if (tileColor == null)
        {
            Debug.LogWarning($"[ColorMatchCondition] 타일에 Color가 할당되지 않았습니다.");
            return false;
        }

        RcColorSO diceBottomColor = pawn.GetBottomColor();
        return diceBottomColor == tileColor;
    }
}
