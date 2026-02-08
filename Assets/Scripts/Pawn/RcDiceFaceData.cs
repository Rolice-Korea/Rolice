using UnityEngine;

public struct RcDiceFaceData
{
    // === 면 인덱스 상수 ===
    public const int TOP = 0, BOTTOM = 1, FRONT = 2, BACK = 3, LEFT = 4, RIGHT = 5;

    // === 회전 맵 (방향별 면 재배치) ===
    private static readonly int[][] RollMaps = {
        new[] { BACK, FRONT, TOP, BOTTOM, LEFT, RIGHT },    // UP
        new[] { FRONT, BACK, BOTTOM, TOP, LEFT, RIGHT },    // DOWN
        new[] { LEFT, RIGHT, FRONT, BACK, BOTTOM, TOP },    // RIGHT
        new[] { RIGHT, LEFT, FRONT, BACK, TOP, BOTTOM }     // LEFT
    };

    public RcColorSO[] faces;

    public RcDiceFaceData(RcColorSO[] initialFaces)
    {
        if (initialFaces == null || initialFaces.Length != 6)
        {
            Debug.LogError("[DiceFaceData] 면 배열은 6개여야 합니다!");
            faces = new RcColorSO[6];
        }
        else
        {
            faces = (RcColorSO[])initialFaces.Clone();
        }
    }

    public RcDiceFaceData Rotate(Vector2Int direction)
    {
        int mapIndex = GetMapIndex(direction);
        int[] map = RollMaps[mapIndex];

        RcColorSO[] newFaces = new RcColorSO[6];
        for (int i = 0; i < 6; i++)
        {
            newFaces[i] = faces[map[i]];
        }

        return new RcDiceFaceData(newFaces);
    }

    public RcColorSO GetFaceColor(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < 6)
            return faces[faceIndex];

        Debug.LogError($"[DiceFaceData] 잘못된 면 인덱스: {faceIndex}");
        return null;
    }

    public RcColorSO GetBottomColor() => faces[BOTTOM];

    private static int GetMapIndex(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 0;
        if (direction == Vector2Int.down) return 1;
        if (direction == Vector2Int.right) return 2;
        if (direction == Vector2Int.left) return 3;

        Debug.LogWarning($"[DiceFaceData] 알 수 없는 방향: {direction}");
        return 0;
    }
}
