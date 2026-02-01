using UnityEngine;
using UnityEditor;

/// <summary>
/// RcLevelDataSO 커스텀 인스펙터
/// Scene View에서 타일을 시각적으로 표시
/// </summary>
[CustomEditor(typeof(RcLevelDataSO))]
public class RcLevelDataInspector : Editor
{
    private RcLevelDataSO level;
    private bool showTiles = true;
    private bool showRules = true;
    
    void OnEnable()
    {
        level = (RcLevelDataSO)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // === Header ===
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Rolice Level Data", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(5);
        
        // === Quick Actions ===
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Open in Editor", GUILayout.Height(30)))
            {
                RcLevelEditorWindow.OpenWindow();
            }
            
            if (GUILayout.Button("Validate", GUILayout.Height(30)))
            {
                ValidateLevel();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // === 기본 정보 ===
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Map Settings", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        
        int width = EditorGUILayout.IntField("Width", level.Width);
        int height = EditorGUILayout.IntField("Height", level.Height);
        
        if (EditorGUI.EndChangeCheck())
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            
            if (EditorUtility.DisplayDialog(
                "Resize Map",
                $"맵 크기를 변경하시겠습니까?\n{level.Width}x{level.Height} → {width}x{height}\n\n" +
                "기존 타일 데이터가 손실될 수 있습니다.",
                "변경", "취소"))
            {
                Undo.RecordObject(level, "Resize Map");
                
                // 타일 배열 리사이즈
                ResizeTileArray(level, width, height);
                
                EditorUtility.SetDirty(level);
            }
        }
        
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(5);
        
        // === Game Rules ===
        showRules = EditorGUILayout.Foldout(showRules, "Game Rules", true);
        if (showRules)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawGameRules();
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.Space(5);
        
        // === Tile Array (접을 수 있게) ===
        showTiles = EditorGUILayout.Foldout(showTiles, "Tiles Array (Advanced)", true);
        if (showTiles)
        {
            EditorGUILayout.HelpBox(
                "타일 배치는 Level Editor를 사용하세요.\n" +
                "고급 사용자만 여기서 직접 수정하세요.",
                MessageType.Info
            );
            
            DrawDefaultInspector();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    void DrawGameRules()
    {
        EditorGUI.BeginChangeCheck();
        
        level.Rules.HasTurnLimit = EditorGUILayout.Toggle("Turn Limit", level.Rules.HasTurnLimit);
        
        if (level.Rules.HasTurnLimit)
        {
            EditorGUI.indentLevel++;
            level.Rules.MaxTurns = EditorGUILayout.IntField("Max Turns", level.Rules.MaxTurns);
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(5);
        
        level.Rules.HasTimeLimit = EditorGUILayout.Toggle("Time Limit", level.Rules.HasTimeLimit);
        
        if (level.Rules.HasTimeLimit)
        {
            EditorGUI.indentLevel++;
            level.Rules.MaxTime = EditorGUILayout.FloatField("Max Time (seconds)", level.Rules.MaxTime);
            EditorGUI.indentLevel--;
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(level);
        }
    }
    
    void ValidateLevel()
    {
        int colorTiles = 0;
        int totalTiles = 0;
        
        foreach (var tile in level.Tiles)
        {
            if (tile == null || string.IsNullOrEmpty(tile.TileID))
                continue;
            
            totalTiles++;
            
            if (tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
            {
                colorTiles++;
            }
        }
        
        string message = $"=== Level Validation ===\n\n" +
                        $"Map Size: {level.Width} x {level.Height}\n" +
                        $"Total Tiles: {totalTiles}\n" +
                        $"Color Tiles: {colorTiles}\n\n";
        
        bool hasErrors = false;
        
        if (colorTiles == 0)
        {
            message += "❌ ERROR: 색깔 타일이 없습니다!\n";
            hasErrors = true;
        }
        
        if (level.Rules.HasTurnLimit && colorTiles > level.Rules.MaxTurns)
        {
            message += $"⚠️ WARNING: 색깔 타일({colorTiles})이 턴 제한({level.Rules.MaxTurns})보다 많습니다!\n";
        }
        
        if (!hasErrors)
        {
            message += "✅ 레벨이 유효합니다!";
        }
        
        EditorUtility.DisplayDialog("Validation Result", message, "OK");
        Debug.Log(message);
    }
    
    void ResizeTileArray(RcLevelDataSO level, int newWidth, int newHeight)
    {
        int oldWidth = level.Width;
        int oldHeight = level.Height;
        
        int newSize = newWidth * newHeight;
        RcTileData[] newTiles = new RcTileData[newSize];
        
        // 새 배열 초기화
        for (int i = 0; i < newSize; i++)
        {
            newTiles[i] = new RcTileData { TileID = "" };
        }
        
        // 기존 데이터 복사
        if (level.Tiles != null)
        {
            int copyWidth = Mathf.Min(oldWidth, newWidth);
            int copyHeight = Mathf.Min(oldHeight, newHeight);
            
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    int oldIndex = y * oldWidth + x;
                    int newIndex = y * newWidth + x;
                    
                    if (oldIndex < level.Tiles.Length)
                    {
                        RcTileData oldTile = level.Tiles[oldIndex];
                        if (oldTile != null)
                        {
                            newTiles[newIndex] = oldTile.Clone();
                        }
                    }
                }
            }
        }
        
        level.Width = newWidth;
        level.Height = newHeight;
        level.Tiles = newTiles;
        
        Debug.Log($"[Inspector] 맵 크기 변경: {newWidth}x{newHeight}");
    }
    
    // === Scene View에 시각화 ===
    void OnSceneGUI()
    {
        if (level == null || level.Tiles == null) return;
        
        // 배열 크기 검증
        int expectedSize = level.Width * level.Height;
        if (level.Tiles.Length != expectedSize)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 300, 80));
            EditorGUILayout.HelpBox(
                "타일 배열 크기 오류!\n인스펙터에서 Width/Height를 확인하세요.",
                MessageType.Error
            );
            GUILayout.EndArea();
            Handles.EndGUI();
            return;
        }
        
        // Scene View에 그리드 표시
        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                Vector3 worldPos = new Vector3(x, 0, y);
                RcTileData tile = level.GetTile(x, y);
                
                if (tile != null && !string.IsNullOrEmpty(tile.TileID))
                {
                    // 타일 표시
                    Color tileColor = GetTileColor(tile);
                    Handles.color = tileColor;
                    Handles.DrawSolidDisc(worldPos + Vector3.up * 0.05f, Vector3.up, 0.4f);
                    
                    // 레이블
                    string label = GetTileLabel(tile);
                    Handles.Label(worldPos + Vector3.up * 0.3f, label);
                }
            }
        }
    }
    
    Color GetTileColor(RcTileData tile)
    {
        if (tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
        {
            return new Color(1f, 0.7f, 0.3f, 0.6f); // 색깔 타일
        }
        
        if (tile.TileID.Contains("Teleport"))
        {
            return new Color(0.5f, 0.5f, 1f, 0.6f); // 텔레포트
        }
        
        return new Color(0.7f, 0.7f, 0.7f, 0.6f); // 일반 타일
    }
    
    string GetTileLabel(RcTileData tile)
    {
        if (tile.BehaviorSO != null && tile.BehaviorSO.RequiresClearTracking)
            return "🎨";
        
        if (tile.TileID.Contains("Teleport"))
            return "🌀";
        
        return "□";
    }
}