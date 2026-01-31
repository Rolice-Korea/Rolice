using System;
using System.Collections.Generic;
using Engine;
using UnityEngine;

public class RcLevelManager : RcSingleton<RcLevelManager>
{
    private RcLevelDataSO currentLevelData;
    private Dictionary<Vector2Int, RcTileData> runtimeTiles;
    private HashSet<Vector2Int> colorTilesRemaining;
    private RcTeleportPairManager teleportManager;
    
    public bool IsInitialized { get; private set; }
    
    public event Action OnLevelCompleted;
    public event Action<Vector2Int> OnColorTileCleared;

    public LevelLoadResult LoadLevel(RcLevelDataSO levelData, Transform tilesParent = null)
    {
        // 입력 검증
        if (levelData == null)
            return LevelLoadResult.CreateFailure("LevelData가 null입니다");
        
        if (levelData.Width <= 0 || levelData.Height <= 0)
            return LevelLoadResult.CreateFailure($"잘못된 맵 크기: {levelData.Width}x{levelData.Height}");
        
        try
        {
            ClearLevel();
            InitializeCollections();
            currentLevelData = levelData;
            
            int tilesCreated = GenerateMap(tilesParent);
            
            IsInitialized = true;
            
            return LevelLoadResult.CreateSuccess(tilesCreated, colorTilesRemaining.Count);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LevelManager] 레벨 로드 실패: {e.Message}");
            ClearLevel();
            return LevelLoadResult.CreateFailure(e.Message);
        }
    }
    
    public void ClearLevel()
    {
        runtimeTiles?.Clear();
        colorTilesRemaining?.Clear();
        teleportManager?.Clear();
        
        currentLevelData = null;
        IsInitialized = false;
    }
    
    private void InitializeCollections()
    {
        runtimeTiles = new Dictionary<Vector2Int, RcTileData>();
        colorTilesRemaining = new HashSet<Vector2Int>();
        teleportManager = new RcTeleportPairManager();
    }

    private int GenerateMap(Transform tilesParent)
    {
        int tilesCreated = 0;
        
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
                tilesCreated++;
                
                // 3. 타일 행동 초기화
                InitializeTileBehavior(tileObj, runtimeTile);
                
                // 4. 클리어 추적이 필요한 타일 등록
                if (RequiresClearTracking(runtimeTile))
                {
                    colorTilesRemaining.Add(gridPos);
                    Debug.Log($"[LevelManager] 색깔 타일 등록: {gridPos} (BehaviorSO: {runtimeTile.BehaviorSO.name}, RequiresClearTracking: {runtimeTile.BehaviorSO.RequiresClearTracking})");
                }
            }
        }
        
        Debug.Log($"[LevelManager] === 맵 생성 완료 ===");
        
        return tilesCreated;
    }
    
    private void InitializeTileBehavior(GameObject tileObject, RcTileData tileData)
    {
        if (tileData.BehaviorSO == null) return;
        
        ITileBehavior behavior = tileData.GetBehavior(tileObject);
    }
    
    private bool RequiresClearTracking(RcTileData tile)
    {
        return tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking;
    }
    
    // === 타일 접근 API ===
    
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
        if (!colorTilesRemaining.Remove(pos)) 
            return;
        
        Debug.Log($"[LevelManager] 색깔 타일 클리어: {pos}");
        Debug.Log($"  - 남은 타일: {colorTilesRemaining.Count}개");
        
        OnColorTileCleared?.Invoke(pos);
        
        // 모든 색깔 타일이 클리어되면 레벨 완료
        if (CheckLevelComplete())
        {
            HandleLevelComplete();
        }
    }
    
    /// 레벨 클리어 조건 체크 (모든 색깔 타일이 클리어되었는지)
    public bool CheckLevelComplete()
    {
        bool isComplete = colorTilesRemaining.Count == 0;
        
        if (isComplete)
        {
            Debug.Log("[LevelManager] ✓ 모든 색깔 타일이 클리어되었습니다!");
        }
        
        return isComplete;
    }
    
    private void HandleLevelComplete()
    {
        OnLevelCompleted?.Invoke();
    }
    
    // === 텔레포트 관리 ===
    
    public void RegisterTeleportPair(string pairID, Vector2Int position)
    {
        teleportManager?.Register(pairID, position);
    }
    
    public Vector2Int? FindTeleportPair(string pairID, Vector2Int myPosition)
    {
        return teleportManager?.FindPair(pairID, myPosition);
    }
    
    public void PrintDebugInfo()
    {
        Debug.Log("=== LevelManager 상태 ===");
        Debug.Log($"  - 초기화: {IsInitialized}");
        Debug.Log($"  - 런타임 타일: {runtimeTiles?.Count ?? 0}개");
        Debug.Log($"  - 남은 색깔 타일: {colorTilesRemaining?.Count ?? 0}개");
        Debug.Log($"  - 텔레포트 페어: {teleportManager?.GetPairCount() ?? 0}개");
    }
    
    public int GetRemainingColorTiles()
    {
        return colorTilesRemaining?.Count ?? 0;
    }
}