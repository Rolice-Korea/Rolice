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

    /// <summary>
    /// 주사위 면 방향. X → Y → Z 축 순서, 각 축은 + → - 순서.
    /// 값이 InitialDiceFaces / faceToSlot 등 6칸 배열의 인덱스로 직렬화되므로 순서 변경 금지.
    /// 반대면 = (int)dir ^ 1, 축 = (int)dir >> 1 (0=X, 1=Y, 2=Z).
    /// </summary>
    public enum RcDiceDirectionType
    {
        PlusX  = 0, // Right  (Vector3.right)
        MinusX = 1, // Left   (Vector3.left)
        PlusY  = 2, // Top    (Vector3.up)
        MinusY = 3, // Bottom (Vector3.down)
        PlusZ  = 4, // Front  (Vector3.forward)
        MinusZ = 5, // Back   (Vector3.back)
        Max
    }

    public enum RcFaceSkinType
    {
        Default,
        Wood,
        Stone,
        Metal,
        Water,
        Skin_1, // 면 전체 색상
        Skin_2, // 가운데 원형 비움
        Skin_3, // 가운데 사각 비움
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
