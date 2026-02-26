using UnityEngine;

/// Condition → Action 매핑 규칙
/// 모든 Condition이 통과하면 Actions를 순차 실행
[CreateAssetMenu(fileName = "NewTileRule", menuName = "Rolice/Tile/Rule")]
public class RcTileRuleSO : ScriptableObject
{
    [Header("실행 조건 (모두 충족 시 Actions 실행)")]
    public RcTileConditionSO[] Conditions;

    [Header("실행할 행동")]
    public RcTileActionSO[] Actions;

    /// 조건 체크 후 행동 실행. 실행되었으면 true 반환.
    public bool TryExecute(RcDicePawn pawn, RcTileData tileData)
    {
        if (Conditions != null)
        {
            foreach (var condition in Conditions)
            {
                if (condition == null) continue;
                if (!condition.Evaluate(pawn, tileData))
                    return false;
            }
        }

        if (Actions != null)
        {
            foreach (var action in Actions)
            {
                if (action == null) continue;
                action.Execute(pawn, tileData);
            }
        }

        return true;
    }
}
