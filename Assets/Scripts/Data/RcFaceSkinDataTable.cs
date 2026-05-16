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
}

/// <summary>
/// 스킨 하나에 대응하는 flat 데이터 테이블.
/// 런타임 조회는 GetFaceData()로, 결과는 기존 RcFaceData 구조 그대로.
/// </summary>
[CreateAssetMenu(fileName = "NewFaceSkinDataTable", menuName = "Rolice/Face Skin Data Table")]
public class RcFaceSkinDataTable : RcDataTableSO<RcFaceSkinRow>
{
    [SerializeField] private RcFaceSkinType skinType = RcFaceSkinType.Default;

    private RcFaceData builtData;
    private bool       bBuilt;

    public RcFaceSkinType SkinType => skinType;

    protected override void OnTableChanged() => Rebuild();

    private void Rebuild()
    {
        if (Rows == null || Rows.Length == 0)
        {
            builtData = default;
            bBuilt    = true;
            return;
        }

        var bundles = new RcColorBundle[Rows.Length];
        for (int i = 0; i < Rows.Length; i++)
        {
            var row = Rows[i];
            bundles[i] = new RcColorBundle
            {
                ColorType    = row.ColorType,
                FaceMaterial = row.FaceMaterial,
                TileMaterial = row.TileMaterial,
                MatchEffect  = row.MatchEffect,
            };
        }

        builtData = RcFaceData.Build(skinType, bundles);
        bBuilt    = true;
    }

    public RcFaceData GetFaceData()
    {
        if (!bBuilt) Rebuild();
        return builtData;
    }
}
