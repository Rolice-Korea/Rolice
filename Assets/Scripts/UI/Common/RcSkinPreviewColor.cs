using UnityEngine;

namespace Rolice.UI
{
    /// <summary>
    /// 스킨 슬롯 아이콘에 쓸 대표 색상 추출. 아이콘 스프라이트가 없을 때 머티리얼만으로 스킨을 구분하기 위함.
    /// Material.color(_Color)는 커스텀 셰이더(NeonDice 등)에 없어 쓰지 않는다.
    /// </summary>
    public static class RcSkinPreviewColor
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");

        // 아이콘 대표 면 색. 스킨 간 차이는 몸체 톤(_BaseColor)에서 나므로 한 색만 보면 충분
        private const RcColorType RepresentativeFaceColor = RcColorType.White;

        public static Color FromFaceSkin(RcFaceSkinDataTable table, Color fallback)
        {
            if (table == null) return fallback;
            if (!table.TryGetFaceMaterial(RepresentativeFaceColor, out var mat)) return fallback;
            return Opaque(mat.HasColor(BaseColorId) ? mat.GetColor(BaseColorId) : fallback);
        }

        /// <summary>테두리는 식별 색이 _EdgeColor에 있으므로 우선 사용, 없으면 _BaseColor.</summary>
        public static Color FromEdgeMaterial(Material mat, Color fallback)
        {
            if (mat == null) return fallback;
            if (mat.HasColor(EdgeColorId)) return Opaque(mat.GetColor(EdgeColorId));
            if (mat.HasColor(BaseColorId)) return Opaque(mat.GetColor(BaseColorId));
            return fallback;
        }

        // HDR 값이 섞여도 UI Image에서 정상 표시되도록 [0,1] 클램프 + 불투명
        private static Color Opaque(Color c)
            => new(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);
    }
}
