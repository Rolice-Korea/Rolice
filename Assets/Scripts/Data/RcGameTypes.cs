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
}
