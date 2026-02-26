using UnityEngine;

/// 타일 행동 실행 추상 베이스 클래스
/// 각 구체적 행동은 이를 상속받아 Execute를 구현
public abstract class RcTileActionSO : ScriptableObject
{
    /// 행동 실행
    public abstract void Execute(RcDicePawn pawn, RcTileData tileData);

    /// 타일 생성 시 1회 호출되는 초기화 (필요 시 override)
    public virtual void Initialize(RcTileData tileData) { }
}
