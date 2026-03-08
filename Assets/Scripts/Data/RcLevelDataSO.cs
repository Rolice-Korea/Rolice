using Rolice.Data;
using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Rolice/Level")]
public class RcLevelDataSO : ScriptableObject
{
    [Header("Stage Info")]
    public RcStageInfo StageInfo = new();

    [Header("Map Settings")]
    public int Width;
    public int Height;

    public RcTileData[] Tiles;

    [Header("Dice Setup")]
    public RcColorType[] InitialDiceFaces = new RcColorType[6];

    [Header("Game Rules")]
    public RcLevelRules Rules = new RcLevelRules();

    private void OnValidate()
    {
        int requiredSize = Width * Height;

        if (Tiles != null && Tiles.Length == requiredSize) return;

        RcTileData[] newTiles = new RcTileData[requiredSize];

        if (Tiles != null)
        {
            for (int i = 0; i < Mathf.Min(Tiles.Length, newTiles.Length); i++)
                newTiles[i] = Tiles[i];
        }

        Tiles = newTiles;

        if (InitialDiceFaces == null || InitialDiceFaces.Length != 6)
        {
            var prev = InitialDiceFaces;
            InitialDiceFaces = new RcColorType[6];
            if (prev != null)
                for (int i = 0; i < Mathf.Min(prev.Length, 6); i++)
                    InitialDiceFaces[i] = prev[i];
        }

        Rules?.Validate();
        StageInfo ??= new RcStageInfo();
        StageInfo.Validate();
    }

    public RcTileData GetTile(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Tiles[y * Width + x];
    }
}
