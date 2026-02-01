using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 레벨 검증 유틸리티
/// - 색깔 타일 개수
/// - 텔레포트 페어 검증
/// - 도달 가능성 (모든 색깔 타일에 도달할 수 있는지)
/// - 게임 룰 균형
/// </summary>
public static class RcLevelValidator
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public List<string> Info = new List<string>();
        
        public void AddError(string message)
        {
            Errors.Add(message);
        }
        
        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }
        
        public void AddInfo(string message)
        {
            Info.Add(message);
        }
        
        public override string ToString()
        {
            string result = "=== Level Validation Result ===\n\n";
            
            if (IsValid)
            {
                result += "✅ Level is VALID!\n\n";
            }
            else
            {
                result += "❌ Level has ERRORS!\n\n";
            }
            
            if (Errors.Count > 0)
            {
                result += "ERRORS:\n";
                foreach (var error in Errors)
                {
                    result += $"  ❌ {error}\n";
                }
                result += "\n";
            }
            
            if (Warnings.Count > 0)
            {
                result += "WARNINGS:\n";
                foreach (var warning in Warnings)
                {
                    result += $"  ⚠️ {warning}\n";
                }
                result += "\n";
            }
            
            if (Info.Count > 0)
            {
                result += "INFO:\n";
                foreach (var info in Info)
                {
                    result += $"  ℹ️ {info}\n";
                }
            }
            
            return result;
        }
    }
    
    public static ValidationResult Validate(RcLevelDataSO level)
    {
        var result = new ValidationResult();
        
        if (level == null)
        {
            result.AddError("Level is null!");
            return result;
        }
        
        // 1. 기본 정보 체크
        ValidateBasicInfo(level, result);
        
        // 2. 색깔 타일 체크
        ValidateColorTiles(level, result);
        
        // 3. 텔레포트 페어 체크
        ValidateTeleportPairs(level, result);
        
        // 4. 게임 룰 균형 체크
        ValidateGameRules(level, result);
        
        // 5. 도달 가능성 체크 (선택적 - 시간이 걸릴 수 있음)
        // ValidateReachability(level, result);
        
        return result;
    }
    
    static void ValidateBasicInfo(RcLevelDataSO level, ValidationResult result)
    {
        result.AddInfo($"Map Size: {level.Width} x {level.Height}");
        
        if (level.Width <= 0 || level.Height <= 0)
        {
            result.AddError("Invalid map size!");
            return;
        }
        
        if (level.Tiles == null)
        {
            result.AddError("Tiles array is null!");
            return;
        }
        
        int expectedSize = level.Width * level.Height;
        if (level.Tiles.Length != expectedSize)
        {
            result.AddError($"Tiles array size mismatch! Expected {expectedSize}, got {level.Tiles.Length}");
        }
    }
    
    static void ValidateColorTiles(RcLevelDataSO level, ValidationResult result)
    {
        int colorTileCount = 0;
        int totalTileCount = 0;
        
        for (int i = 0; i < level.Tiles.Length; i++)
        {
            RcTileData tile = level.Tiles[i];
            
            if (tile == null || string.IsNullOrEmpty(tile.TileID))
                continue;
            
            totalTileCount++;
            
            // BehaviorSO가 RequiresClearTracking이면 색깔 타일
            if (tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
            {
                colorTileCount++;
            }
        }
        
        result.AddInfo($"Total Tiles: {totalTileCount}");
        result.AddInfo($"Color Tiles: {colorTileCount}");
        
        if (colorTileCount == 0)
        {
            result.AddError("No color tiles found! Level cannot be completed.");
        }
        
        if (totalTileCount == 0)
        {
            result.AddWarning("Map is empty!");
        }
    }
    
    static void ValidateTeleportPairs(RcLevelDataSO level, ValidationResult result)
    {
        Dictionary<string, List<Vector2Int>> teleportPairs = new Dictionary<string, List<Vector2Int>>();
        
        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                RcTileData tile = level.GetTile(x, y);
                
                if (tile == null || tile.BehaviorSO == null)
                    continue;
                
                // 텔레포트 타일 찾기
                if (tile.BehaviorSO is RcTeleportBehaviorSO teleportBehavior)
                {
                    string pairID = teleportBehavior.pairID;
                    
                    if (!teleportPairs.ContainsKey(pairID))
                    {
                        teleportPairs[pairID] = new List<Vector2Int>();
                    }
                    
                    teleportPairs[pairID].Add(new Vector2Int(x, y));
                }
            }
        }
        
        if (teleportPairs.Count > 0)
        {
            result.AddInfo($"Teleport Pairs: {teleportPairs.Count}");
            
            foreach (var pair in teleportPairs)
            {
                string pairID = pair.Key;
                int count = pair.Value.Count;
                
                if (count < 2)
                {
                    result.AddWarning($"Teleport pair '{pairID}' has only {count} tile(s)! Need at least 2.");
                }
                else if (count > 2)
                {
                    result.AddWarning($"Teleport pair '{pairID}' has {count} tiles. Multiple teleports with same ID!");
                }
            }
        }
    }
    
    static void ValidateGameRules(RcLevelDataSO level, ValidationResult result)
    {
        if (level.Rules == null)
        {
            result.AddWarning("Game rules are null!");
            return;
        }
        
        // 색깔 타일 개수 카운트
        int colorTileCount = 0;
        foreach (var tile in level.Tiles)
        {
            if (tile != null && tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
            {
                colorTileCount++;
            }
        }
        
        // 턴 제한 체크
        if (level.Rules.HasTurnLimit)
        {
            result.AddInfo($"Turn Limit: {level.Rules.MaxTurns}");
            
            if (level.Rules.MaxTurns <= 0)
            {
                result.AddError("Turn limit must be > 0!");
            }
            
            if (colorTileCount > level.Rules.MaxTurns)
            {
                result.AddWarning(
                    $"Color tiles ({colorTileCount}) > Turn limit ({level.Rules.MaxTurns})! " +
                    "Level might be impossible to complete."
                );
            }
            
            // 여유 턴 체크
            int margin = level.Rules.MaxTurns - colorTileCount;
            if (margin < 3 && colorTileCount > 0)
            {
                result.AddInfo($"Tight difficulty: only {margin} extra turns available.");
            }
        }
        
        // 시간 제한 체크
        if (level.Rules.HasTimeLimit)
        {
            result.AddInfo($"Time Limit: {level.Rules.MaxTime} seconds");
            
            if (level.Rules.MaxTime <= 0)
            {
                result.AddError("Time limit must be > 0!");
            }
        }
    }
    
    /// <summary>
    /// BFS를 사용하여 시작 위치에서 모든 색깔 타일에 도달할 수 있는지 체크
    /// 시작 위치는 (0,0)으로 가정 (실제로는 레벨에 시작 위치 정보 추가 필요)
    /// </summary>
    static void ValidateReachability(RcLevelDataSO level, ValidationResult result)
    {
        // 시작 위치 찾기 (0,0에 타일이 있으면 그곳에서 시작)
        Vector2Int startPos = Vector2Int.zero;
        
        RcTileData startTile = level.GetTile(startPos.x, startPos.y);
        if (startTile == null || string.IsNullOrEmpty(startTile.TileID))
        {
            // 첫 번째 타일 찾기
            bool found = false;
            for (int y = 0; y < level.Height && !found; y++)
            {
                for (int x = 0; x < level.Width && !found; x++)
                {
                    RcTileData tile = level.GetTile(x, y);
                    if (tile != null && !string.IsNullOrEmpty(tile.TileID))
                    {
                        startPos = new Vector2Int(x, y);
                        found = true;
                    }
                }
            }
            
            if (!found)
            {
                result.AddWarning("No start position found!");
                return;
            }
        }
        
        // BFS로 도달 가능한 타일 찾기
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(startPos);
        visited.Add(startPos);
        
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                
                if (next.x < 0 || next.x >= level.Width || next.y < 0 || next.y >= level.Height)
                    continue;
                
                if (visited.Contains(next))
                    continue;
                
                RcTileData tile = level.GetTile(next.x, next.y);
                
                if (tile == null || string.IsNullOrEmpty(tile.TileID))
                    continue;
                
                if (!tile.bCanEnter)
                    continue;
                
                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        
        // 도달 불가능한 색깔 타일 찾기
        List<Vector2Int> unreachableColorTiles = new List<Vector2Int>();
        
        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                RcTileData tile = level.GetTile(x, y);
                
                if (tile != null && tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
                {
                    if (!visited.Contains(pos))
                    {
                        unreachableColorTiles.Add(pos);
                    }
                }
            }
        }
        
        if (unreachableColorTiles.Count > 0)
        {
            result.AddError($"Found {unreachableColorTiles.Count} unreachable color tile(s)!");
            foreach (var pos in unreachableColorTiles)
            {
                result.AddError($"  - Color tile at ({pos.x}, {pos.y}) is unreachable!");
            }
        }
        else
        {
            result.AddInfo("All color tiles are reachable from start position.");
        }
    }
}
