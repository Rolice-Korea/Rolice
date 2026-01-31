using System.Collections.Generic;
using Engine;
using UnityEngine;

/// <summary>
/// 레벨의 모든 데이터와 생명주기를 관리하는 중앙 매니저
/// - 맵 생성을 주도하고 제어
/// - 런타임 타일 데이터와 GameObject를 함께 관리
/// - 타일 접근 API 제공
/// - 게임 로직 (클리어 조건 등) 처리
/// </summary>
public class RcLevelManager : RcSingleton<RcLevelManager>
{
    private RcLevelDataSO currentLevelData;
    
    private Dictionary<Vector2Int, RcTileData> runtimeTiles;
    
    private HashSet<Vector2Int> colorTilesRemaining;
    
    // 텔레포트 페어 관리 (pairID -> 타일 위치들)
    private Dictionary<string, List<Vector2Int>> teleportPairs;
    
    public bool IsInitialized { get; private set; }
    
    public void LoadLevel(RcLevelDataSO levelData, Transform tilesParent = null)
    {
        if (levelData == null)
        {
            Debug.LogError("[LevelManager] LevelData가 null입니다!");
            return;
        }
        
        ClearLevel();
        
        currentLevelData = levelData;
        runtimeTiles = new Dictionary<Vector2Int, RcTileData>();
        colorTilesRemaining = new HashSet<Vector2Int>();
        teleportPairs = new Dictionary<string, List<Vector2Int>>();
        
        GenerateMap(tilesParent);
        
        IsInitialized = true;
        
        Debug.Log($"[LevelManager] 레벨 로드 완료: {levelData.name} ({levelData.Width}x{levelData.Height})");
    }
    
    private void GenerateMap(Transform tilesParent)
    {
        for (int y = 0; y < currentLevelData.Height; y++)
        {
            for (int x = 0; x < currentLevelData.Width; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                RcTileData sourceTile = currentLevelData.GetTile(x, y);
                
                // 빈 타일은 스킵
                if (sourceTile == null || string.IsNullOrEmpty(sourceTile.TileID))
                    continue;
                
                // 1. 런타임 타일 데이터 생성 (원본 복사)
                RcTileData runtimeTile = sourceTile.Clone();
                runtimeTiles[gridPos] = runtimeTile;
                
                // 2. GameObject 생성
                GameObject tileObj = RcMapGenerator.CreateTile(sourceTile.TileID, gridPos, tilesParent);
                
                if (tileObj == null)
                {
                    Debug.LogError($"[LevelManager] 타일 생성 실패: {sourceTile.TileID} at ({x}, {y})");
                    continue;
                }
                
                runtimeTile.Setup(tileObj);
                
                InitializeTileBehavior(tileObj, runtimeTile);
                
                if (IsColorTile(runtimeTile))
                {
                    colorTilesRemaining.Add(gridPos);
                }
            }
        }
    }
    
    private void InitializeTileBehavior(GameObject tileObject, RcTileData tileData)
    {
        if (tileData.BehaviorSO == null) return;
        
        ITileBehavior behavior = tileData.GetBehavior(tileObject);
        
        if (behavior != null)
        {
            Debug.Log($"[LevelManager] 타일 행동 초기화: {tileObject.name} - {tileData.BehaviorSO.name}");
        }
    }
    
    private void ClearLevel()
    {
        runtimeTiles?.Clear();
        colorTilesRemaining?.Clear();
        teleportPairs?.Clear();
        
        currentLevelData = null;
        IsInitialized = false;
    }
    
    public RcTileData GetRuntimeTile(Vector2Int pos)
    {
        return runtimeTiles != null && runtimeTiles.TryGetValue(pos, out var tile) ? tile : null;
    }
    
    public bool HasTile(Vector2Int pos)
    {
        return runtimeTiles != null && runtimeTiles.ContainsKey(pos);
    }
    
    public void ClearColorTile(Vector2Int pos)
    {
        if (!colorTilesRemaining.Remove(pos)) return;
        
        Debug.Log($"[LevelManager] 색깔 타일 클리어: {pos} (남은 타일: {colorTilesRemaining.Count})");
            
        // 클리어 조건 체크
        if (CheckLevelComplete())
        {
            OnLevelComplete();
        }
    }
    
    public bool CheckLevelComplete()
    {
        return colorTilesRemaining.Count == 0;
    }
    
    private void OnLevelComplete()
    {
        Debug.Log("=== 레벨 클리어! ===");
        // TODO: 이벤트 발행 또는 승리 UI 표시
        // RcEventHub.Publish(GameEvent.LevelComplete);
    }
    
    private bool IsColorTile(RcTileData tile)
    {
        // BehaviorSO가 ColorMatch 타입인지 확인
        return tile.BehaviorSO is RcColorMatchBehaviorSO;
    }

    // ========================================
    // 텔레포트 페어 관리
    // ========================================
    
    /// <summary>
    /// 텔레포트 타일을 페어에 등록합니다
    /// </summary>
    public void RegisterTeleportPair(string pairID, Vector2Int position)
    {
        if (string.IsNullOrEmpty(pairID))
        {
            Debug.LogWarning("[LevelManager] PairID가 비어있습니다!");
            return;
        }
        
        if (!teleportPairs.ContainsKey(pairID))
        {
            teleportPairs[pairID] = new List<Vector2Int>();
        }
        
        if (!teleportPairs[pairID].Contains(position))
        {
            teleportPairs[pairID].Add(position);
            Debug.Log($"[LevelManager] 텔레포트 페어 등록: {pairID} at {position}");
        }
    }
    
    /// <summary>
    /// 같은 pairID를 가진 다른 타일의 위치를 찾습니다
    /// </summary>
    public Vector2Int? FindTeleportPair(string pairID, Vector2Int myPosition)
    {
        if (!teleportPairs.TryGetValue(pairID, out var positions))
            return null;
        
        // 자기 자신을 제외한 첫 번째 타일 반환
        foreach (var pos in positions)
        {
            if (pos != myPosition)
                return pos;
        }
        
        return null;
    }
 
    public void PrintDebugInfo()
    {
        Debug.Log($"[LevelManager] 런타임 타일: {runtimeTiles?.Count ?? 0}개");
        Debug.Log($"[LevelManager] 남은 색깔 타일: {colorTilesRemaining?.Count ?? 0}개");
        Debug.Log($"[LevelManager] 텔레포트 페어: {teleportPairs?.Count ?? 0}개");
    }
}