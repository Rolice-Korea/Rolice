using Rolice.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Rolice/Level")]
public class RcLevelDataSO : ScriptableObject
{
    [Header("Stage Info")]
    public RcStageInfo StageInfo = new();

    [Header("Map Settings")]
    public int Width;
    public int Height;

    [SerializeReference, RcPolymorphicReference(typeof(RcTileData))]
    public RcTileData[] Tiles;

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
            {
                newTiles[i] = Tiles[i]; // null = 빈 타일 (SerializeReference에서 유효)
            }
        }

        Tiles = newTiles;

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
