using UnityEngine;

/// 타일 조건 판단 추상 베이스 클래스
/// 각 구체적 조건은 이를 상속받아 Evaluate를 구현
public abstract class RcTileConditionSO : ScriptableObject
{
    /// 조건 판단: 주어진 pawn과 tileData로 조건 충족 여부를 반환
    public abstract bool Evaluate(RcDicePawn pawn, RcTileData tileData);
}
