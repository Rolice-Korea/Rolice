using System;

namespace Rolice
{
    public enum RcColorType
    {
        None = 0,
        White,
        Magenta,
        Yellow,
        Green,
        Cyan,
        Grey,
        Max
    }

    public enum RcFaceSkinType
    {
        Default,
        Wood,
        Stone,
        Metal,
        Water,
        Max
    }

    public enum RcEdgeSkinType
    {
		Default,
        Bronze,
        Silver,
        Gold,
        Max
    }

    public enum RcScreenMode
    {
        Landscape = 0,
        Portrait  = 1,
    }

    public enum RcBagTabType
    {
        Face,
        Edge
    }

    public enum RcShopTabType
    {
        Face,
        Edge
    }

    public enum RcItemType
    {
        FaceSkin,
        EdgeSkin,
    }

    /// <summary>
    /// 메인 상점 스킨의 결제 방식.
    /// 별(Star)은 결제 화폐가 아니라 해금 게이트(RequiredStars)이므로 비용 타입에 포함하지 않는다.
    /// </summary>
    public enum RcShopCostType
    {
        Free, // 해금 조건만 충족하면 무상 획득
        Gem,  // 잼 소모
    }
}
