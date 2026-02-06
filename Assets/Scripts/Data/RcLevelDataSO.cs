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
                newTiles[i] = Tiles[i];
            }
        }
            
        for (int i = 0; i < newTiles.Length; i++)
        {
            newTiles[i] ??= new RcTileData { TileID = "" };
        }
            
        Tiles = newTiles;
        
        // 규칙 검증
        if (Rules != null)
        {
            Rules.Validate();
        }

        // 스테이지 정보 검증
        StageInfo ??= new RcStageInfo();
        StageInfo.Validate();
    }
    
    public RcTileData GetTile(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Tiles[y * Width + x];
    }
}