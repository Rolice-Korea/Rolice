using UnityEngine;
using Rolice;

/// <summary>
/// 주사위 6면 색상 상태. faces 인덱스 = RcDiceDirectionType (PlusX, MinusX, PlusY, MinusY, PlusZ, MinusZ).
/// </summary>
public struct RcDiceFaceData
{
    private const int FaceCount = RcDiceDirectionTypeExt.Count;

    private const int PX = (int)RcDiceDirectionType.PlusX;
    private const int NX = (int)RcDiceDirectionType.MinusX;
    private const int PY = (int)RcDiceDirectionType.PlusY;
    private const int NY = (int)RcDiceDirectionType.MinusY;
    private const int PZ = (int)RcDiceDirectionType.PlusZ;
    private const int NZ = (int)RcDiceDirectionType.MinusZ;

    // 굴림 후 newFaces[i] = faces[map[i]]. 각 배열은 RcDiceDirectionType 순서.
    private static readonly int[][] RollMaps = {
        new[] { PX, NX, NZ, PZ, PY, NY },    // UP    : 위 ← 뒤, 아래 ← 앞, 앞 ← 위, 뒤 ← 아래
        new[] { PX, NX, PZ, NZ, NY, PY },    // DOWN  : 위 ← 앞, 아래 ← 뒤, 앞 ← 아래, 뒤 ← 위
        new[] { PY, NY, NX, PX, PZ, NZ },    // RIGHT : 오른 ← 위, 왼 ← 아래, 위 ← 왼, 아래 ← 오른
        new[] { NY, PY, PX, NX, PZ, NZ }     // LEFT  : 오른 ← 아래, 왼 ← 위, 위 ← 오른, 아래 ← 왼
    };

    public RcColorType[] faces;

    public RcDiceFaceData(RcColorType[] initialFaces)
    {
        if (initialFaces == null || initialFaces.Length != FaceCount)
        {
            Debug.LogError("[DiceFaceData] 면 배열은 6개여야 합니다!");
            faces = new RcColorType[FaceCount];
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

        RcColorType[] newFaces = new RcColorType[FaceCount];
        for (int i = 0; i < FaceCount; i++)
        {
            newFaces[i] = faces[map[i]];
        }

        return new RcDiceFaceData(newFaces);
    }

    public RcDiceFaceData ChangeColor(RcColorType targetColorType, RcColorType newColorType)
    {
        if (targetColorType == newColorType) return this;

        RcColorType[] newFaces = new RcColorType[FaceCount];
        for (int i = 0; i < FaceCount; i++)
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

    public RcColorType GetFaceColor(RcDiceDirectionType direction) => GetFaceColor((int)direction);

    public RcColorType GetFaceColor(int faceIndex)
    {
        if (faceIndex >= 0 && faceIndex < FaceCount)
            return faces[faceIndex];

        return RcColorType.None;
    }

    public RcColorType GetBottomColor() => faces[NY];

    private static int GetMapIndex(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 0;
        if (direction == Vector2Int.down) return 1;
        if (direction == Vector2Int.right) return 2;
        if (direction == Vector2Int.left) return 3;
        return 0;
    }
}
