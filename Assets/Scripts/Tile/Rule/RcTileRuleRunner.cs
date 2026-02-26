using UnityEngine;

/// 타일 프리팹에 부착하는 Rule 실행 컴포넌트
/// Rules 배열을 순회하며 조건 체크 후 행동 실행
public class RcTileRuleRunner : MonoBehaviour
{
    [Header("타일 규칙 목록")]
    public RcTileRuleSO[] Rules;

    [Header("클리어 추적")]
    [Tooltip("이 타일이 클리어 조건에 포함되는지 여부")]
    public bool RequiresClearTracking = false;

    /// 주사위 진입 시 호출 — Rules 순회하며 조건 체크 후 행동 실행
    public void OnDiceEnter(RcDicePawn pawn, RcTileData tileData)
    {
        if (Rules == null) return;

        foreach (var rule in Rules)
        {
            if (rule == null) continue;
            rule.TryExecute(pawn, tileData);
        }
    }

    /// 타일 생성 시 1회 호출 — 각 Action의 Initialize 실행
    public void Initialize(RcTileData tileData)
    {
        if (Rules == null) return;

        foreach (var rule in Rules)
        {
            if (rule?.Actions == null) continue;
            foreach (var action in rule.Actions)
            {
                if (action == null) continue;
                action.Initialize(tileData);
            }
        }
    }
}
