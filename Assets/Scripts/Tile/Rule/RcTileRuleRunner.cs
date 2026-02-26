using UnityEngine;

/// 타일 프리팹에 동적으로 추가되는 Rule 실행 컴포넌트
/// LevelManager가 AddComponent 후 Setup()으로 구성한다
public class RcTileRuleRunner : MonoBehaviour
{
    private RcTileRuleSO[] rules;
    private bool requiresClearTracking;

    public bool RequiresClearTracking => requiresClearTracking;

    /// LevelManager가 타일 생성 직후 1회 호출 — 규칙 구성 + Action 초기화를 원자적으로 처리
    public void Initialize(RcTileRuleSO[] tileRules, bool clearTracking, RcTileData tileData)
    {
        rules = tileRules;
        requiresClearTracking = clearTracking;

        if (rules == null) return;

        foreach (var rule in rules)
        {
            if (rule?.Actions == null) continue;
            foreach (var action in rule.Actions)
                action?.Initialize(tileData);
        }
    }

    /// 주사위 진입 시 호출 — Rules 순회하며 조건 체크 후 행동 실행
    public void OnDiceEnter(RcDicePawn pawn, RcTileData tileData)
    {
        if (rules == null) return;

        foreach (var rule in rules)
            rule?.TryExecute(pawn, tileData);
    }

}
