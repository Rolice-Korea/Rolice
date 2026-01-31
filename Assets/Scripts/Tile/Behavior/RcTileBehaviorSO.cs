using UnityEngine;

/// <summary>
/// 타일 행동을 정의하는 ScriptableObject 베이스 클래스
/// 각 특수 행동은 이를 상속받아 구현
/// </summary>
public abstract class RcTileBehaviorSO : ScriptableObject
{
    /// <summary>
    /// 런타임에 행동 인스턴스 생성
    /// </summary>
    /// <param name="tileObject">타일 GameObject</param>
    /// <param name="tileData">타일 데이터</param>
    /// <returns>생성된 행동 인스턴스</returns>
    public abstract ITileBehavior CreateBehavior(GameObject tileObject, RcTileData tileData);
}
