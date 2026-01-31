using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Rolice/Level")]
public class RcLevelDataSO : ScriptableObject
{
    public int Width;
    public int Height;
    
    public RcTileData[] Tiles;
    
    private void OnValidate()
    {
        int requiredSize = Width * Height;

        if (Tiles != null && Tiles.Length == requiredSize) return;
        
        RcTileData[] newTiles = new RcTileData[requiredSize];
            
        // 기존 데이터 보존
        if (Tiles != null)
        {
            for (int i = 0; i < Mathf.Min(Tiles.Length, newTiles.Length); i++)
            {
                newTiles[i] = Tiles[i];
            }
        }
            
        // 빈 칸은 새 TileData로 초기화
        for (int i = 0; i < newTiles.Length; i++)
        {
            newTiles[i] ??= new RcTileData { TileID = "" };
        }
            
        Tiles = newTiles;
    }
    
    public RcTileData GetTile(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        return Tiles[y * Width + x];
    }
}
