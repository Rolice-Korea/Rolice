using System;
using UnityEngine;
using Rolice;

/// <summary>
/// 스킨 하나에 속한 색상별 머티리얼/이펙트 데이터. SkinType은 테이블 헤더로 보유.
/// </summary>
[Serializable]
public struct RcFaceSkinRow
{
    [RcColumn(105f)] public RcColorType ColorType;
    [RcColumn(168f)] public Material    FaceMaterial;
    [RcColumn(168f)] public Material    TileMaterial;
    [RcColumn(140f)] public GameObject  MatchEffect;
    [RcColumn(150f)] public Sprite      IconSprite; // 추가: 키 외의 모든 데이터 필드는 행 구조체 내부에 열로 위치시킵니다.
}

/// <summary>
/// 스킨 하나에 대응하는 flat 데이터 테이블.
/// 런타임 조회 및 헬퍼 메서드가 직접 내장되어 있어 별도의 중간 변환 구조체가 불필요합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewFaceSkinDataTable", menuName = "Rolice/Face Skin Data Table")]
public class RcFaceSkinDataTable : RcDataTableSO<RcFaceSkinRow>
{
    [SerializeField] private RcFaceSkinType skinType = RcFaceSkinType.Default;

    public RcFaceSkinType SkinType => skinType;

    // 테이블 행 구조체 내부에 삽입된 IconSprite를 대표로 반환합니다.
    public Sprite IconSprite => (Rows != null && Rows.Length > 0) ? Rows[0].IconSprite : null;

    public Material GetFaceMaterial(RcColorType colorType)
    {
        if (Rows == null) return null;
        foreach (var row in Rows)
            if (row.ColorType == colorType) return row.FaceMaterial;
        Debug.LogWarning($"[RcFaceSkinDataTable] FaceMaterial not found for ColorType: {colorType} in Skin: {skinType}");
        return null;
    }

    public Material GetTileMaterial(RcColorType colorType)
    {
        if (Rows == null) return null;
        foreach (var row in Rows)
            if (row.ColorType == colorType) return row.TileMaterial;
        Debug.LogWarning($"[RcFaceSkinDataTable] TileMaterial not found for ColorType: {colorType} in Skin: {skinType}");
        return null;
    }

    public GameObject GetMatchEffect(RcColorType colorType)
    {
        if (Rows == null) return null;
        foreach (var row in Rows)
            if (row.ColorType == colorType) return row.MatchEffect;
        Debug.LogWarning($"[RcFaceSkinDataTable] MatchEffect not found for ColorType: {colorType} in Skin: {skinType}");
        return null;
    }
}
