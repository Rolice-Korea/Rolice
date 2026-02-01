using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Rolice 레벨 에디터 - 메인 에디터 창
/// Scene View에서 직접 타일 배치 및 편집 가능
/// </summary>
public class RcLevelEditorWindow : EditorWindow
{
    // === 에디터 상태 ===
    private RcLevelDataSO currentLevel;
    private Vector2 scrollPos;
    private bool showGrid = true;
    private bool showValidation = true;
    
    // === 타일 팔레트 ===
    private string selectedTileID = "normal";
    private ColorType selectedColor = ColorType.Gray;
    private string teleportPairID = "TP_01";
    private RcTileBehaviorSO selectedBehavior;
    
    // === 편집 모드 ===
    private EditMode editMode = EditMode.Place;
    
    // === 타일 프리팹 캐싱 ===
    private Dictionary<string, GameObject> tilePreviewCache = new Dictionary<string, GameObject>();
    
    private enum EditMode
    {
        Place,      // 타일 배치
        Erase,      // 타일 삭제
        Paint       // 속성 변경
    }
    
    [MenuItem("Tools/Rolice/Level Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<RcLevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        LoadTilePreviews();
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    void OnGUI()
    {
        try
        {
            DrawHeader();
            
            if (currentLevel == null)
            {
                DrawNoLevelSelected();
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            {
                // 좌측: 타일 팔레트
                DrawTilePalette();
                
                GUILayout.Space(10);
                
                // 우측: 레벨 정보 & 그리드
                DrawLevelEditor();
            }
            EditorGUILayout.EndHorizontal();
            
            DrawFooter();
        }
        catch (System.Exception e)
        {
            // GUI 레이아웃 복구
            GUIUtility.ExitGUI();
            
            Debug.LogError($"[LevelEditor] GUI 에러: {e.Message}\n{e.StackTrace}");
        }
    }
    
    // ========================================
    // Header
    // ========================================
    
    void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("Rolice Level Editor", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Current Level:", GUILayout.Width(100));
            
            RcLevelDataSO newLevel = (RcLevelDataSO)EditorGUILayout.ObjectField(
                currentLevel, 
                typeof(RcLevelDataSO), 
                false
            );
            
            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button("New Level", GUILayout.Width(80)))
            {
                CreateNewLevel();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawNoLevelSelected()
    {
        EditorGUILayout.HelpBox(
            "레벨을 선택하거나 새로 만들어주세요.\n\n" +
            "1. 상단에서 기존 레벨 선택\n" +
            "2. 'New Level' 버튼으로 새 레벨 생성",
            MessageType.Info
        );
    }
    
    // ========================================
    // Tile Palette (좌측)
    // ========================================
    
    void DrawTilePalette()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(220));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🎨 Tile Palette", EditorStyles.boldLabel);
        
        // === 편집 모드 ===
        GUILayout.Space(5);
        GUILayout.Label("Edit Mode:", EditorStyles.miniBoldLabel);
        
        editMode = (EditMode)GUILayout.Toolbar((int)editMode, new string[] { "Place", "Erase", "Paint" });
        
        GUILayout.Space(10);
        
        if (editMode != EditMode.Erase)
        {
            // === 타일 타입 선택 ===
            GUILayout.Label("Tile Type:", EditorStyles.miniBoldLabel);
            
            DrawTileButton("normal", "일반 타일", Color.gray);
            DrawTileButton("color", "색깔 타일", new Color(1f, 0.7f, 0.3f));
            DrawTileButton("teleport", "텔레포트", new Color(0.5f, 0.5f, 1f));
            
            GUILayout.Space(10);
            
            // === 타일별 옵션 ===
            if (selectedTileID == "color")
            {
                DrawColorTileOptions();
            }
            else if (selectedTileID == "teleport")
            {
                DrawTeleportOptions();
            }
            
            GUILayout.Space(10);
            
            // === Behavior SO 직접 할당 ===
            GUILayout.Label("Tile Behavior (Optional):", EditorStyles.miniBoldLabel);
            selectedBehavior = (RcTileBehaviorSO)EditorGUILayout.ObjectField(
                selectedBehavior,
                typeof(RcTileBehaviorSO),
                false
            );
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
    
    void DrawTileButton(string tileID, string label, Color color)
    {
        Color oldBg = GUI.backgroundColor;
        
        if (selectedTileID == tileID)
        {
            GUI.backgroundColor = Color.green;
        }
        else
        {
            GUI.backgroundColor = color;
        }
        
        if (GUILayout.Button(label, GUILayout.Height(35)))
        {
            selectedTileID = tileID;
        }
        
        GUI.backgroundColor = oldBg;
    }
    
    void DrawColorTileOptions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Color Options:", EditorStyles.miniBoldLabel);
        
        selectedColor = (ColorType)EditorGUILayout.EnumPopup("Color:", selectedColor);
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawTeleportOptions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Teleport Options:", EditorStyles.miniBoldLabel);
        
        teleportPairID = EditorGUILayout.TextField("Pair ID:", teleportPairID);
        
        EditorGUILayout.HelpBox(
            "같은 Pair ID를 가진 타일끼리 텔레포트됩니다.\n" +
            "예: TP_01, TP_02, TP_03...",
            MessageType.Info
        );
        
        GUILayout.Space(5);
        
        // 빠른 선택 버튼
        GUILayout.Label("빠른 선택:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("TP_01", EditorStyles.miniButton))
                teleportPairID = "TP_01";
            if (GUILayout.Button("TP_02", EditorStyles.miniButton))
                teleportPairID = "TP_02";
            if (GUILayout.Button("TP_03", EditorStyles.miniButton))
                teleportPairID = "TP_03";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("TP_04", EditorStyles.miniButton))
                teleportPairID = "TP_04";
            if (GUILayout.Button("TP_05", EditorStyles.miniButton))
                teleportPairID = "TP_05";
            if (GUILayout.Button("TP_06", EditorStyles.miniButton))
                teleportPairID = "TP_06";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    // ========================================
    // Level Editor (우측)
    // ========================================
    
    void DrawLevelEditor()
    {
        EditorGUILayout.BeginVertical();
        
        // === 레벨 설정 ===
        DrawLevelSettings();
        
        GUILayout.Space(10);
        
        // === 게임 룰 ===
        DrawGameRules();
        
        GUILayout.Space(10);
        
        // === 그리드 미니맵 ===
        if (showGrid)
        {
            DrawGridMinimap();
        }
        
        GUILayout.Space(10);
        
        // === 검증 결과 ===
        if (showValidation)
        {
            DrawValidation();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawLevelSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("⚙️ Level Settings", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        
        int newWidth = EditorGUILayout.IntField("Width:", currentLevel.Width);
        int newHeight = EditorGUILayout.IntField("Height:", currentLevel.Height);
        
        if (EditorGUI.EndChangeCheck())
        {
            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);
            
            if (EditorUtility.DisplayDialog(
                "맵 크기 변경",
                $"맵 크기를 {currentLevel.Width}x{currentLevel.Height} → {newWidth}x{newHeight}로 변경하시겠습니까?\n" +
                "기존 타일 데이터가 손실될 수 있습니다.",
                "변경",
                "취소"))
            {
                Undo.RecordObject(currentLevel, "Resize Level");
                
                // 즉시 타일 배열 리사이즈
                ResizeTileArray(newWidth, newHeight);
                
                EditorUtility.SetDirty(currentLevel);
                SceneView.RepaintAll();
                Repaint();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 타일 배열을 새 크기로 리사이즈 (기존 데이터 최대한 보존)
    /// </summary>
    void ResizeTileArray(int newWidth, int newHeight)
    {
        int oldWidth = currentLevel.Width;
        int oldHeight = currentLevel.Height;
        
        int newSize = newWidth * newHeight;
        RcTileData[] newTiles = new RcTileData[newSize];
        
        // 새 배열 초기화
        for (int i = 0; i < newSize; i++)
        {
            newTiles[i] = new RcTileData { TileID = "" };
        }
        
        // 기존 데이터 복사 (겹치는 부분만)
        if (currentLevel.Tiles != null)
        {
            int copyWidth = Mathf.Min(oldWidth, newWidth);
            int copyHeight = Mathf.Min(oldHeight, newHeight);
            
            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    int oldIndex = y * oldWidth + x;
                    int newIndex = y * newWidth + x;
                    
                    if (oldIndex < currentLevel.Tiles.Length)
                    {
                        RcTileData oldTile = currentLevel.Tiles[oldIndex];
                        if (oldTile != null)
                        {
                            newTiles[newIndex] = oldTile.Clone();
                        }
                    }
                }
            }
        }
        
        // 새 크기와 타일 배열 적용
        currentLevel.Width = newWidth;
        currentLevel.Height = newHeight;
        currentLevel.Tiles = newTiles;
        
        Debug.Log($"[LevelEditor] 맵 크기 변경 완료: {newWidth}x{newHeight} (총 {newSize}개 타일)");
    }
    
    void DrawGameRules()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🎮 Game Rules", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        
        currentLevel.Rules.HasTurnLimit = EditorGUILayout.Toggle("Turn Limit:", currentLevel.Rules.HasTurnLimit);
        
        if (currentLevel.Rules.HasTurnLimit)
        {
            currentLevel.Rules.MaxTurns = EditorGUILayout.IntField("  Max Turns:", currentLevel.Rules.MaxTurns);
        }
        
        currentLevel.Rules.HasTimeLimit = EditorGUILayout.Toggle("Time Limit:", currentLevel.Rules.HasTimeLimit);
        
        if (currentLevel.Rules.HasTimeLimit)
        {
            currentLevel.Rules.MaxTime = EditorGUILayout.FloatField("  Max Time (s):", currentLevel.Rules.MaxTime);
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(currentLevel);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawGridMinimap()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🗺️ Grid Minimap", EditorStyles.boldLabel);
        
        // 타일 배열이 맵 크기와 맞는지 체크
        int expectedSize = currentLevel.Width * currentLevel.Height;
        if (currentLevel.Tiles == null || currentLevel.Tiles.Length != expectedSize)
        {
            EditorGUILayout.HelpBox(
                $"타일 배열 크기가 맞지 않습니다!\n" +
                $"예상: {expectedSize}, 실제: {currentLevel.Tiles?.Length ?? 0}\n" +
                "레벨 에셋을 다시 저장해주세요.",
                MessageType.Warning
            );
            EditorGUILayout.EndVertical();
            return;
        }
        
        const float cellSize = 20f;
        float gridWidth = currentLevel.Width * cellSize;
        float gridHeight = currentLevel.Height * cellSize;
        
        Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        
        // 배경
        EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));
        
        // 타일 그리기
        for (int y = 0; y < currentLevel.Height; y++)
        {
            for (int x = 0; x < currentLevel.Width; x++)
            {
                RcTileData tile = currentLevel.GetTile(x, y);
                
                Rect cellRect = new Rect(
                    gridRect.x + x * cellSize,
                    gridRect.y + (currentLevel.Height - 1 - y) * cellSize,
                    cellSize - 1,
                    cellSize - 1
                );
                
                Color cellColor = GetTileColor(tile);
                EditorGUI.DrawRect(cellRect, cellColor);
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    Color GetTileColor(RcTileData tile)
    {
        if (tile == null || string.IsNullOrEmpty(tile.TileID))
            return new Color(0.1f, 0.1f, 0.1f);
        
        if (tile.TileID == "ColorTile")
            return new Color(1f, 0.7f, 0.3f, 0.8f);
        
        if (tile.TileID == "TeleportTile")
            return new Color(0.5f, 0.5f, 1f, 0.8f);
        
        return new Color(0.5f, 0.5f, 0.5f, 0.8f);
    }
    
    void DrawValidation()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("✅ Validation", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Validate Level"))
        {
            ValidateLevel();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    // ========================================
    // Footer (하단 액션 버튼들)
    // ========================================
    
    void DrawFooter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        {
            showGrid = GUILayout.Toggle(showGrid, "Show Grid", EditorStyles.miniButton, GUILayout.Width(80));
            showValidation = GUILayout.Toggle(showValidation, "Validation", EditorStyles.miniButton, GUILayout.Width(80));
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear All", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Clear All", "모든 타일을 삭제하시겠습니까?", "삭제", "취소"))
                {
                    ClearAllTiles();
                }
            }
            
            if (GUILayout.Button("Save", GUILayout.Width(80)))
            {
                AssetDatabase.SaveAssets();
                Debug.Log("Level saved!");
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    // ========================================
    // Scene View 편집
    // ========================================
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (currentLevel == null) return;
        
        // Scene View 상단에 컨트롤 UI 표시
        DrawSceneControls();
        
        // 그리드 그리기 (클릭 가능)
        DrawSceneGrid();
    }
    
    void DrawSceneControls()
    {
        Handles.BeginGUI();
        
        GUILayout.BeginArea(new Rect(10, 10, 250, 140));
        GUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🎨 Level Editor", EditorStyles.boldLabel);
        
        GUILayout.Space(5);
        
        // 현재 모드 표시
        string modeText = editMode switch
        {
            EditMode.Place => GetPlaceModeText(),
            EditMode.Erase => "삭제 모드",
            EditMode.Paint => "페인트 모드",
            _ => "알 수 없음"
        };
        
        GUILayout.Label($"모드: {modeText}", EditorStyles.miniLabel);
        
        if (editMode == EditMode.Place)
        {
            if (selectedTileID == "color")
            {
                GUILayout.Label($"색깔: {selectedColor}", EditorStyles.miniLabel);
            }
            else if (selectedTileID == "teleport")
            {
                GUILayout.Label($"채널: {teleportPairID}", EditorStyles.miniLabel);
            }
        }
        
        GUILayout.Space(5);
        
        GUILayout.Label("좌클릭: 배치/삭제", EditorStyles.miniLabel);
        GUILayout.Label("우클릭: 삭제", EditorStyles.miniLabel);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
        
        Handles.EndGUI();
    }
    
    string GetPlaceModeText()
    {
        return selectedTileID switch
        {
            "normal" => "배치: 일반 타일",
            "color" => $"배치: 색깔 타일 ({selectedColor})",
            "teleport" => $"배치: 텔레포트 ({teleportPairID})",
            _ => $"배치: {selectedTileID}"
        };
    }
    
    void DrawSceneGrid()
    {
        // 타일 배열 유효성 체크
        int expectedSize = currentLevel.Width * currentLevel.Height;
        if (currentLevel.Tiles == null || currentLevel.Tiles.Length != expectedSize)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            EditorGUILayout.HelpBox(
                "타일 배열 크기 오류!\n레벨 에셋을 다시 저장해주세요.",
                MessageType.Error
            );
            GUILayout.EndArea();
            Handles.EndGUI();
            return;
        }
        
        for (int y = 0; y < currentLevel.Height; y++)
        {
            for (int x = 0; x < currentLevel.Width; x++)
            {
                Vector3 worldPos = new Vector3(x, 0, y);
                RcTileData tile = currentLevel.GetTile(x, y);
                
                // 클릭 가능한 셀 버튼 (가장 먼저!)
                DrawClickableCell(x, y, worldPos);
                
                // 그리드 셀 테두리
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                DrawGridCell(worldPos);
                
                // 타일이 있으면 표시
                if (tile != null && !string.IsNullOrEmpty(tile.TileID))
                {
                    DrawTileVisual(worldPos, tile);
                }
                
                // 좌표 라벨
                Handles.Label(worldPos + Vector3.up * 0.1f, $"({x},{y})", EditorStyles.miniLabel);
            }
        }
    }
    
    void DrawClickableCell(int x, int y, Vector3 worldPos)
    {
        // 클릭 가능한 영역 (보이지 않는 버튼)
        float buttonSize = 0.45f;
        Vector3 buttonPos = worldPos + Vector3.up * 0.01f;
        
        // Handles.Button으로 클릭 감지
        Handles.color = new Color(1, 1, 1, 0.01f); // 거의 투명
        
        if (Handles.Button(buttonPos, Quaternion.Euler(90, 0, 0), buttonSize, buttonSize, Handles.RectangleHandleCap))
        {
            Event e = Event.current;
            
            // 좌클릭: 배치, 우클릭: 삭제
            if (e.button == 0)
            {
                if (editMode == EditMode.Place)
                {
                    PlaceTile(x, y);
                }
                else if (editMode == EditMode.Erase)
                {
                    RemoveTile(x, y);
                }
                else if (editMode == EditMode.Paint)
                {
                    PaintTile(x, y);
                }
            }
            else if (e.button == 1)
            {
                RemoveTile(x, y);
            }
            
            SceneView.RepaintAll();
            Repaint();
        }
    }
    
    void DrawGridCell(Vector3 center)
    {
        float size = 0.5f;
        Vector3[] corners = new Vector3[]
        {
            center + new Vector3(-size, 0, -size),
            center + new Vector3(size, 0, -size),
            center + new Vector3(size, 0, size),
            center + new Vector3(-size, 0, size)
        };
        
        Handles.DrawLine(corners[0], corners[1]);
        Handles.DrawLine(corners[1], corners[2]);
        Handles.DrawLine(corners[2], corners[3]);
        Handles.DrawLine(corners[3], corners[0]);
    }
    
    void DrawTileVisual(Vector3 worldPos, RcTileData tile)
    {
        Color tileColor = GetTileColor(tile);
        tileColor.a = 0.6f;
        
        Handles.color = tileColor;
        Handles.DrawSolidDisc(worldPos + Vector3.up * 0.05f, Vector3.up, 0.4f);
        
        // 타일 타입 표시
        Handles.Label(worldPos + Vector3.up * 0.2f, GetTileLabel(tile), EditorStyles.whiteLabel);
    }
    
    string GetTileLabel(RcTileData tile)
    {
        if (tile == null || string.IsNullOrEmpty(tile.TileID))
            return "";
        
        if (tile.TileID == "ColorTile")
            return "🎨";
        
        if (tile.TileID == "TeleportTile")
            return "🌀";
        
        return "□";
    }
    
    // ========================================
    // 타일 편집 액션
    // ========================================
    
    void PlaceTile(int x, int y)
    {
        Undo.RecordObject(currentLevel, "Place Tile");
        
        RcTileData tile = currentLevel.GetTile(x, y);
        
        if (tile == null)
        {
            tile = new RcTileData();
            currentLevel.Tiles[y * currentLevel.Width + x] = tile;
        }
        
        // 타일 ID 설정
        if (selectedTileID == "color")
        {
            // 색깔 타일: "color_red", "color_blue" 등
            string colorName = selectedColor.ToString().ToLower();
            tile.TileID = $"color_{colorName}";
        }
        else
        {
            // 일반 타일, 텔레포트
            tile.TileID = selectedTileID;
        }
        
        tile.bCanEnter = true;
        
        // Behavior 설정
        if (selectedTileID == "color")
        {
            tile.BehaviorSO = GetColorMatchBehavior(selectedColor);
        }
        else if (selectedTileID == "teleport")
        {
            tile.BehaviorSO = GetOrCreateTeleportBehavior(teleportPairID);
        }
        else if (selectedBehavior != null)
        {
            tile.BehaviorSO = selectedBehavior;
        }
        else
        {
            tile.BehaviorSO = null;
        }
        
        EditorUtility.SetDirty(currentLevel);
        
        Debug.Log($"[LevelEditor] 타일 배치: ({x},{y}) - {tile.TileID}");
    }
    
    void RemoveTile(int x, int y)
    {
        Undo.RecordObject(currentLevel, "Remove Tile");
        
        RcTileData tile = currentLevel.GetTile(x, y);
        
        if (tile != null)
        {
            tile.TileID = "";
            tile.BehaviorSO = null;
        }
        
        EditorUtility.SetDirty(currentLevel);
    }
    
    void PaintTile(int x, int y)
    {
        RcTileData tile = currentLevel.GetTile(x, y);
        
        if (tile == null || string.IsNullOrEmpty(tile.TileID))
            return;
        
        Undo.RecordObject(currentLevel, "Paint Tile");
        
        if (selectedBehavior != null)
        {
            tile.BehaviorSO = selectedBehavior;
        }
        
        EditorUtility.SetDirty(currentLevel);
    }
    
    void ClearAllTiles()
    {
        Undo.RecordObject(currentLevel, "Clear All Tiles");
        
        for (int i = 0; i < currentLevel.Tiles.Length; i++)
        {
            if (currentLevel.Tiles[i] != null)
            {
                currentLevel.Tiles[i].TileID = "";
                currentLevel.Tiles[i].BehaviorSO = null;
            }
        }
        
        EditorUtility.SetDirty(currentLevel);
        SceneView.RepaintAll();
    }
    
    // ========================================
    // Behavior SO 헬퍼
    // ========================================
    
    RcTileBehaviorSO GetColorMatchBehavior(ColorType color)
    {
        // Resources 폴더에서 찾기 (실제로는 프로젝트 구조에 맞게 수정)
        string path = $"Behaviors/ColorMatch_{color}";
        var behavior = Resources.Load<RcColorMatchBehaviorSO>(path);
        
        if (behavior == null)
        {
            // 없으면 첫 번째 ColorMatchBehavior 사용
            string[] guids = AssetDatabase.FindAssets("t:RcColorMatchBehaviorSO");
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                behavior = AssetDatabase.LoadAssetAtPath<RcColorMatchBehaviorSO>(assetPath);
            }
        }
        
        return behavior;
    }
    
    RcTileBehaviorSO GetOrCreateTeleportBehavior(string pairID)
    {
        // 먼저 해당 Pair ID를 가진 Behavior 찾기
        string[] guids = AssetDatabase.FindAssets("t:RcTeleportBehaviorSO");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var behavior = AssetDatabase.LoadAssetAtPath<RcTeleportBehaviorSO>(path);
            
            if (behavior != null && behavior.pairID == pairID)
            {
                return behavior;
            }
        }
        
        // 없으면 새로 만들기
        Debug.Log($"[LevelEditor] 텔레포트 Behavior 생성: {pairID}");
        
        RcTeleportBehaviorSO newBehavior = CreateInstance<RcTeleportBehaviorSO>();
        newBehavior.pairID = pairID;
        
        // Behaviors 폴더에 저장
        string savePath = $"Assets/ScriptableObjects/Behaviors/Teleport_{pairID}.asset";
        
        // 폴더가 없으면 생성
        string folderPath = "Assets/ScriptableObjects/Behaviors";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Behaviors");
        }
        
        AssetDatabase.CreateAsset(newBehavior, savePath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[LevelEditor] Behavior 저장: {savePath}");
        
        return newBehavior;
    }
    
    RcTileBehaviorSO GetTeleportBehavior(string pairID)
    {
        // 하위 호환성을 위해 남겨둠
        return GetOrCreateTeleportBehavior(pairID);
    }
    
    // ========================================
    // 유틸리티
    // ========================================
    
    void CreateNewLevel()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Level",
            "NewLevel",
            "asset",
            "새 레벨을 저장할 위치를 선택하세요"
        );
        
        if (string.IsNullOrEmpty(path)) return;
        
        RcLevelDataSO newLevel = CreateInstance<RcLevelDataSO>();
        newLevel.Width = 7;
        newLevel.Height = 7;
        newLevel.Tiles = new RcTileData[newLevel.Width * newLevel.Height];
        
        // 빈 타일로 초기화
        for (int i = 0; i < newLevel.Tiles.Length; i++)
        {
            newLevel.Tiles[i] = new RcTileData { TileID = "" };
        }
        
        newLevel.Rules = new LevelRules
        {
            HasTurnLimit = true,
            MaxTurns = 15,
            HasTimeLimit = false
        };
        
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        
        currentLevel = newLevel;
        
        Debug.Log($"새 레벨 생성: {path}");
    }
    
    void LoadTilePreviews()
    {
        // 타일 프리팹 미리 로드 (옵션)
        GameObject normalTile = Resources.Load<GameObject>("Tiles/normal");
        GameObject teleportTile = Resources.Load<GameObject>("Tiles/teleport");
        
        if (normalTile != null) tilePreviewCache["normal"] = normalTile;
        if (teleportTile != null) tilePreviewCache["teleport"] = teleportTile;
        
        // 색깔 타일들
        foreach (ColorType color in System.Enum.GetValues(typeof(ColorType)))
        {
            string colorName = color.ToString().ToLower();
            GameObject colorTile = Resources.Load<GameObject>($"Tiles/color_{colorName}");
            if (colorTile != null)
            {
                tilePreviewCache[$"color_{colorName}"] = colorTile;
            }
        }
    }
    
    void ValidateLevel()
    {
        if (currentLevel == null) return;
        
        int colorTiles = 0;
        int teleportTiles = 0;
        int normalTiles = 0;
        
        for (int i = 0; i < currentLevel.Tiles.Length; i++)
        {
            RcTileData tile = currentLevel.Tiles[i];
            
            if (tile == null || string.IsNullOrEmpty(tile.TileID))
                continue;
            
            if (tile.TileID == "ColorTile") colorTiles++;
            else if (tile.TileID == "TeleportTile") teleportTiles++;
            else normalTiles++;
        }
        
        string report = $"=== Level Validation ===\n" +
                       $"Size: {currentLevel.Width}x{currentLevel.Height}\n" +
                       $"Normal Tiles: {normalTiles}\n" +
                       $"Color Tiles: {colorTiles}\n" +
                       $"Teleport Tiles: {teleportTiles}\n" +
                       $"Total: {normalTiles + colorTiles + teleportTiles}\n\n";
        
        if (currentLevel.Rules.HasTurnLimit)
        {
            report += $"Turn Limit: {currentLevel.Rules.MaxTurns}\n";
            
            if (colorTiles > currentLevel.Rules.MaxTurns)
            {
                report += "⚠️ Warning: Color tiles > Turn limit!\n";
            }
        }
        
        if (colorTiles == 0)
        {
            report += "❌ Error: No color tiles! Level cannot be completed.\n";
        }
        
        Debug.Log(report);
        EditorUtility.DisplayDialog("Validation Result", report, "OK");
    }
}