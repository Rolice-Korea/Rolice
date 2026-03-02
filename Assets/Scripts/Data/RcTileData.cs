using System;
using UnityEngine;

[Serializable]
public class RcTileData
{
    [Header("Tile Type")]
    [Tooltip("이 타일의 타입 정의 SO. null이면 빈 타일(이동 불가)")]
    public RcTileTypeSO TileType;

    public bool bCanEnter = true;

    [Header("Runtime (에디터 무시)")]
    [NonSerialized] public GameObject TileObject;
    [NonSerialized] public RcTileRuleRunner Runner;

    public bool IsEmpty => TileType == null;

    public void Setup(GameObject tileObject, RcTileRuleRunner runner)
    {
        TileObject = tileObject;
        Runner = runner;
    }

    /// 타일 오브젝트 생성 직후 1회 호출 — 서브클래스에서 비주얼 초기화
    public virtual void InitializeVisual(GameObject tileObject) { }

    public bool CanEnter(RcDicePawn pawn)
    {
        return bCanEnter;
    }

    public virtual RcTileData Clone()
    {
        return new RcTileData
        {
            TileType = this.TileType,
            bCanEnter = this.bCanEnter
        };
    }
}
