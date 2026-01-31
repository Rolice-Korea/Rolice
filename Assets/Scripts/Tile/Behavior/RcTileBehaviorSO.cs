using UnityEngine;

/// 타일 행동을 정의하는 ScriptableObject 베이스 클래스
/// 각 특수 행동은 이를 상속받아 구현
public abstract class RcTileBehaviorSO : ScriptableObject
{
    [Header("Behavior Metadata")]
    [Tooltip("이 타일이 클리어 조건에 포함되는지 여부")]
    public bool RequiresClearTracking = false;

    /// 런타임에 행동 인스턴스 생성
    public abstract ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData);
}