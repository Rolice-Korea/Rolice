using UnityEngine;
using Rolice;

public struct RcDiceFaceData
{
    public const int TOP = 0, BOTTOM = 1, FRONT = 2, BACK = 3, LEFT = 4, RIGHT = 5;

    private static readonly int[][] RollMaps = {
        new[] { BACK, FRONT, TOP, BOTTOM, LEFT, RIGHT },    // UP
        new[] { FRONT, BACK, BOTTOM, TOP, LEFT, RIGHT },    // DOWN
        new[] { LEFT, RIGHT, FRONT, BACK, BOTTOM, TOP },    // RIGHT
        new[] { RIGHT, LEFT, FRONT, BACK, TOP, BOTTOM }     // LEFT
    };

    public RcColorType[] faces;

    public RcDiceFaceData(RcColorType[] initialFaces)
    {
        if (initialFaces == null || initialFaces.Length != 6)
        {
            Debug.LogError("[DiceFaceData] 면 배열은 6개여야 합니다!");
            faces = new RcColorType[6];
        }
        else
        {
            faces = (RcColorType[])initialFaces.Clone();
        }
    }

    public RcDiceFaceData Rotate(Vector2Int direction)
    {
        int mapIndex = GetMapIndex(direction);
        int[] map = RollMaps[mapIndex];

        RcColorType[] newFaces = new RcColorType[6];
        for (int i = 0; i < 6; i++)
        {
            newFaces[i] = faces[map[i]];
        }

        return new RcDiceFaceData(newFaces);
    }

    public RcDiceFaceData ChangeColor(RcColorType targetColorType, RcColorType newColorType)
    {
        if (targetColorType == newColorType) return this;

        RcColorType[] newFaces = new RcColorType[6];
        for (int i = 0; i < 6; i++)
        {
            if (faces[i] == targetColorType)
            {
                newFaces[i] = newColorType;
            }
            else
            {
                newFaces[i] = faces[i];
            }
        }

        return new RcDiceFaceData(newFaces);
    }

    public RcColorType GetFaceColor(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < 6)
            return faces[faceIndex];

        return RcColorType.None;
    }

    public RcColorType GetBottomColor() => faces[BOTTOM];

    private static int GetMapIndex(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 0;
        if (direction == Vector2Int.down) return 1;
        if (direction == Vector2Int.right) return 2;
        if (direction == Vector2Int.left) return 3;
        return 0;
    }
}
