using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rolice;

[CustomEditor(typeof(RcLevelEditorProxy))]
public class RcLevelEditorProxyEditor : Editor
{
    private RcTileTypeSO brushTileType;
    private RcColorType brushColor = RcColorType.None;

    private RcTileTypeSO[] allTileTypes = {};
    private readonly RcColorType[] allColors = { 
        RcColorType.White, 
        RcColorType.Magenta, 
        RcColorType.Yellow, 
        RcColorType.Green, 
        RcColorType.Cyan, 
        RcColorType.Grey 
    };

    private Vector2Int hoveredCell   = new(-1, -1);
    private Vector2Int lastPainted   = new(-1, -1); 
    private Vector2Int selectedCell  = new(-1, -1); 
    private int        activeTab     = 0;            
    private RcLevelEditorProxy proxy;
    private SerializedObject levelDataSO;

    void OnEnable()
    {
        proxy = target as RcLevelEditorProxy;
        if (proxy == null) return;

        RefreshAssetCache();
        RestoreBrushState();
        activeTab = SessionState.GetInt("RcLevelEditor.ActiveTab", 0);
        UpdateLevelDataSO();

        proxy.RebuildScene();
    }

    void OnDisable()
    {
        SaveBrushState();
    }

    public override void OnInspectorGUI()
    {
        if (proxy == null) return;

        serializedObject.Update();

        DrawLevelSection();

        if (proxy.LevelData != null)
        {
            EditorGUILayout.Space(6);
            DrawMapSection();
            EditorGUILayout.Space(6);
            DrawRulesSection();
            EditorGUILayout.Space(6);
            DrawStarsSection();
            EditorGUILayout.Space(6);
            DrawDiceSetupSection();
            EditorGUILayout.Space(6);

            int newTab = GUILayout.Toolbar(activeTab, new[] { "Paint", "Edit" }, GUILayout.Height(26));
            if (newTab != activeTab)
            {
                activeTab    = newTab;
                selectedCell = new(-1, -1);
                SessionState.SetInt("RcLevelEditor.ActiveTab", activeTab);
                SceneView.RepaintAll();
            }
            EditorGUILayout.Space(4);

            if (activeTab == 0)
                DrawBrushSection();
            else
                DrawEditSection();

            EditorGUILayout.Space(8);
            DrawActionsSection();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawLevelSection()
    {
        EditorGUILayout.LabelField("Level Asset", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var picked = (RcLevelDataSO)EditorGUILayout.ObjectField(proxy.LevelData, typeof(RcLevelDataSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(proxy, "Change Level");
            proxy.LevelData  = picked;
            selectedCell     = new(-1, -1);
            UpdateLevelDataSO();
            EditorUtility.SetDirty(proxy);
            proxy.RebuildScene();
        }
    }

    void DrawMapSection()
    {
        var ld = proxy.LevelData;
        EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int w = Mathf.Max(1, EditorGUILayout.IntField("Width",  ld.Width));
        int h = Mathf.Max(1, EditorGUILayout.IntField("Height", ld.Height));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ld, "Resize Level");
            ld.Width  = w;
            ld.Height = h;
            EnsureTilesArray(ld);
            EditorUtility.SetDirty(ld);
            proxy.RebuildScene();
        }
    }

    void DrawRulesSection()
    {
        var ld = proxy.LevelData;
        EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        bool hasLimit = EditorGUILayout.Toggle("Turn Limit", ld.Rules.HasTurnLimit);
        int  maxTurns = ld.Rules.MaxTurns;
        if (hasLimit)
            maxTurns = Mathf.Max(1, EditorGUILayout.IntField("Max Turns", maxTurns));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ld, "Edit Rules");
            ld.Rules.HasTurnLimit = hasLimit;
            ld.Rules.MaxTurns     = maxTurns;
            EditorUtility.SetDirty(ld);
        }
    }

    void DrawStarsSection()
    {
        var info = proxy.LevelData.StageInfo;
        if (info?.StarThresholds == null || info.StarThresholds.Length == 0) return;

        EditorGUILayout.LabelField("Stars (턴 이하면 획득)", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        string[] labels = { "★★★  ≤", "★★  ≤", "★  ≤" };
        for (int i = 0; i < Mathf.Min(info.StarThresholds.Length, labels.Length); i++)
            info.StarThresholds[i] = EditorGUILayout.IntField(labels[i], info.StarThresholds[i]);
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(proxy.LevelData);
    }

    static readonly string[] FaceNames = { "TOP", "BOTTOM", "FRONT", "BACK", "LEFT", "RIGHT" };

    void DrawDiceSetupSection()
    {
        var ld = proxy.LevelData;
        if (ld.InitialDiceFaces == null || ld.InitialDiceFaces.Length != 6)
            ld.InitialDiceFaces = new RcColorType[6];

        EditorGUILayout.LabelField("Dice Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("전부 비워두면 프리팹 기본값 사용", MessageType.None);

        var prevBg = GUI.backgroundColor;

        for (int i = 0; i < 6; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(FaceNames[i], GUILayout.Width(58));

            GUI.backgroundColor = ld.InitialDiceFaces[i] == RcColorType.None ? Color.yellow : new Color(0.6f, 0.6f, 0.6f);
            if (GUILayout.Button("None", GUILayout.Height(22), GUILayout.Width(46)))
            {
                Undo.RecordObject(ld, "Dice Face None");
                ld.InitialDiceFaces[i] = RcColorType.None;
                EditorUtility.SetDirty(ld);
            }

            foreach (var color in allColors)
            {
                bool selected    = ld.InitialDiceFaces[i] == color;
                var  swatchColor = GetSwatchColor(color);
                GUI.backgroundColor = selected
                    ? Color.Lerp(swatchColor, Color.yellow, 0.45f)
                    : swatchColor;

                string label = selected ? $"● {color}" : color.ToString();
                if (GUILayout.Button(label, GUILayout.Height(22)))
                {
                    Undo.RecordObject(ld, "Dice Face Color");
                    ld.InitialDiceFaces[i] = color;
                    EditorUtility.SetDirty(ld);
                }
            }

            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(2);
        GUI.backgroundColor = new Color(0.8f, 0.6f, 0.6f);
        if (GUILayout.Button("Clear All (프리팹 기본값)", GUILayout.Height(20)))
        {
            Undo.RecordObject(ld, "Dice Faces Clear");
            ld.InitialDiceFaces = new RcColorType[6];
            EditorUtility.SetDirty(ld);
        }
        GUI.backgroundColor = prevBg;
    }

    void DrawBrushSection()
    {
        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);

        var prevBg = GUI.backgroundColor;

        GUI.backgroundColor = brushTileType == null ? Color.yellow : Color.gray;
        if (GUILayout.Button("Erase", GUILayout.Height(24)))
            SelectBrush(null, RcColorType.None);
        GUI.backgroundColor = prevBg;

        EditorGUILayout.Space(2);

        foreach (var tileType in allTileTypes)
        {
            if (tileType == null) continue;

            EditorGUILayout.LabelField(tileType.name, EditorStyles.miniLabel);

            if (tileType.bHasColor)
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var color in allColors)
                {
                    bool selected = brushTileType == tileType && brushColor == color;
                    var  swatchColor = GetSwatchColor(color);

                    GUI.backgroundColor = selected
                        ? Color.Lerp(swatchColor, Color.yellow, 0.5f)
                        : swatchColor;

                    string label = selected ? $"● {color}" : color.ToString();
                    if (GUILayout.Button(label, GUILayout.Height(26)))
                        SelectBrush(tileType, color);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                bool selected = brushTileType == tileType && brushColor == RcColorType.None;
                GUI.backgroundColor = selected ? Color.yellow : prevBg;
                if (GUILayout.Button(tileType.name, GUILayout.Height(26)))
                    SelectBrush(tileType, RcColorType.None);
            }

            GUI.backgroundColor = prevBg;
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.HelpBox("좌클릭·드래그: 배치   우클릭: 씬뷰에서 바로 선택 후 배치", MessageType.None);
    }

    void SelectBrush(RcTileTypeSO tileType, RcColorType color)
    {
        brushTileType = tileType;
        brushColor    = color;
        SaveBrushState();
        Repaint();
    }

    void DrawActionsSection()
    {
        if (GUILayout.Button("Rebuild Preview"))
            proxy.RebuildScene();

        EditorGUILayout.Space(4);

        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("Save Asset", GUILayout.Height(32)))
            Save();
        GUI.backgroundColor = prev;
    }

    void OnSceneGUI()
    {
        if (proxy == null || proxy.LevelData == null) return;

        UpdateHover();
        DrawOverlay();
        HandleInput();

        if (Event.current.type == EventType.MouseMove)
            SceneView.RepaintAll();
    }

    void UpdateHover()
    {
        Vector2Int cell = ScreenToGrid(Event.current.mousePosition);
        hoveredCell = IsValidCell(cell) ? cell : new Vector2Int(-1, -1);
    }

    void DrawOverlay()
    {
        var ld = proxy.LevelData;
        const float Y = 0.02f;

        Handles.color = new Color(1f, 1f, 1f, 0.18f);
        for (int x = 0; x <= ld.Width; x++)
            Handles.DrawLine(new Vector3(x - 0.5f, Y, -0.5f), new Vector3(x - 0.5f, Y, ld.Height - 0.5f));
        for (int z = 0; z <= ld.Height; z++)
            Handles.DrawLine(new Vector3(-0.5f, Y, z - 0.5f), new Vector3(ld.Width - 0.5f, Y, z - 0.5f));

        Handles.color = new Color(0f, 0f, 0f, 0.35f);
        for (int y = 0; y < ld.Height; y++)
        for (int x = 0; x < ld.Width;  x++)
        {
            var td = ld.GetTile(x, y);
            if (td == null || td.IsEmpty)
                DrawCellQuad(new Vector2Int(x, y), Y);
        }

        if (IsValidCell(hoveredCell))
        {
            Handles.color = new Color(1f, 1f, 0f, 0.28f);
            DrawCellQuad(hoveredCell, Y);
        }

        if (activeTab == 1 && IsValidCell(selectedCell))
        {
            Handles.color = new Color(0f, 0.8f, 1f, 0.45f);
            DrawCellQuad(selectedCell, Y);
        }
    }

    void HandleInput()
    {
        Event e = Event.current;

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout)
            HandleUtility.AddDefaultControl(controlId);

        if (e.alt) return;

        if (activeTab == 0)
            HandlePaintInput(e);
        else
            HandleEditInput(e);
    }

    void HandlePaintInput(Event e)
    {
        if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
        {
            if (IsValidCell(hoveredCell) && hoveredCell != lastPainted)
            {
                PlaceTile(hoveredCell, brushTileType, brushColor);
                lastPainted = hoveredCell;
            }
            e.Use();
        }

        if (e.button == 0 && e.type == EventType.MouseUp)
        {
            lastPainted = new(-1, -1);
            e.Use();
        }

        if (e.button == 1 && e.type == EventType.MouseDown && IsValidCell(hoveredCell))
        {
            ShowContextMenu(hoveredCell);
            e.Use();
        }
    }

    void HandleEditInput(Event e)
    {
        if (e.button == 0 && e.type == EventType.MouseDown && IsValidCell(hoveredCell))
        {
            selectedCell = hoveredCell;
            e.Use();
            Repaint();
        }
    }

    void ShowContextMenu(Vector2Int cell)
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Erase"), false, () =>
        {
            SelectBrush(null, RcColorType.None);
            PlaceTile(cell, null, RcColorType.None);
        });
        menu.AddSeparator("");

        foreach (var tileType in allTileTypes)
        {
            if (tileType == null) continue;
            var capturedType = tileType;

            if (tileType.bHasColor)
            {
                foreach (var color in allColors)
                {
                    var capturedColor = color;
                    bool isActive = brushTileType == capturedType && brushColor == capturedColor;
                    menu.AddItem(new GUIContent($"{tileType.name}/{color}"), isActive, () =>
                    {
                        SelectBrush(capturedType, capturedColor);
                        PlaceTile(cell, capturedType, capturedColor);
                    });
                }
            }
            else
            {
                bool isActive = brushTileType == capturedType && brushColor == RcColorType.None;
                menu.AddItem(new GUIContent(tileType.name), isActive, () =>
                    {
                        SelectBrush(capturedType, RcColorType.None);
                        PlaceTile(cell, capturedType, RcColorType.None);
                    });
            }
        }

        menu.ShowAsContext();
    }

    void DrawEditSection()
    {
        if (!IsValidCell(selectedCell))
        {
            EditorGUILayout.HelpBox("씬뷰에서 셀을 클릭해 선택하세요.", MessageType.Info);
            return;
        }

        var ld    = proxy.LevelData;
        int index = selectedCell.y * ld.Width + selectedCell.x;

        EditorGUILayout.LabelField($"Cell  ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);

        var tileData = ld.GetTile(selectedCell.x, selectedCell.y);
        if (tileData == null)
        {
            EditorGUILayout.HelpBox("빈 셀입니다.", MessageType.None);
            return;
        }

        UpdateLevelDataSO();
        if (levelDataSO == null) return;

        levelDataSO.Update();
        var cellProp = levelDataSO.FindProperty("Tiles").GetArrayElementAtIndex(index);

        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("TileType"));
        if (tileData.TileType != null)
        {
            if (tileData.TileType.bHasColor)
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("Color"));
            if (tileData.TileType.bHasTeleport)
            {
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("TeleportTileID"));
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("TeleportTargetID"));
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            levelDataSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(ld);
            proxy.RefreshTileAt(selectedCell);
        }
    }

    void PlaceTile(Vector2Int cell, RcTileTypeSO tileType, RcColorType color)
    {
        var ld = proxy.LevelData;
        Undo.RecordObject(ld, "Paint Tile");

        EnsureTilesArray(ld);
        int index = cell.y * ld.Width + cell.x;
        ld.Tiles[index] = BuildTileData(tileType, color);

        EditorUtility.SetDirty(ld);
        proxy.RefreshTileAt(cell);
        Repaint();
    }

    static RcTileData BuildTileData(RcTileTypeSO tileType, RcColorType color)
    {
        if (tileType == null) return null;

        var data = new RcTileData { TileType = tileType };
        if (tileType.bHasColor)
            data.Color = color;

        return data;
    }

    void RefreshAssetCache()
    {
        allTileTypes = LoadAllAssets<RcTileTypeSO>();
    }

    static T[] LoadAllAssets<T>() where T : ScriptableObject
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(a => a != null)
            .ToArray();
    }

    void UpdateLevelDataSO()
    {
        if (levelDataSO == null || levelDataSO.targetObject != proxy.LevelData)
            levelDataSO = proxy.LevelData != null ? new SerializedObject(proxy.LevelData) : null;
    }

    bool IsValidCell(Vector2Int cell)
    {
        var ld = proxy.LevelData;
        return ld != null && cell.x >= 0 && cell.x < ld.Width && cell.y >= 0 && cell.y < ld.Height;
    }

    static Vector2Int ScreenToGrid(Vector2 mousePos)
    {
        Ray   ray   = HandleUtility.GUIPointToWorldRay(mousePos);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float dist)) return new Vector2Int(-1, -1);
        Vector3 wp = ray.GetPoint(dist);
        return new Vector2Int(Mathf.RoundToInt(wp.x), Mathf.RoundToInt(wp.z));
    }

    static void DrawCellQuad(Vector2Int cell, float y)
    {
        const float PAD = 0.47f;
        Vector3 c = new Vector3(cell.x, y, cell.y);
        Handles.DrawSolidRectangleWithOutline(
            new[]
            {
                c + new Vector3(-PAD, 0, -PAD),
                c + new Vector3( PAD, 0, -PAD),
                c + new Vector3( PAD, 0,  PAD),
                c + new Vector3(-PAD, 0,  PAD),
            },
            Handles.color,
            Color.clear);
    }

    static void EnsureTilesArray(RcLevelDataSO ld)
    {
        int required = ld.Width * ld.Height;
        if (ld.Tiles != null && ld.Tiles.Length == required) return;

        var next = new RcTileData[required];
        if (ld.Tiles != null)
            for (int i = 0; i < Mathf.Min(ld.Tiles.Length, required); i++)
                next[i] = ld.Tiles[i];
        ld.Tiles = next;
    }

    void Save()
    {
        if (proxy.LevelData == null) return;
        EditorUtility.SetDirty(proxy.LevelData);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LevelEditor] '{proxy.LevelData.name}' 저장 완료.");
    }

    static Color GetSwatchColor(RcColorType color)
    {
        return color switch
        {
            RcColorType.White => Color.white,
            RcColorType.Magenta => Color.magenta,
            RcColorType.Yellow => Color.yellow,
            RcColorType.Green => Color.green,
            RcColorType.Cyan => Color.cyan,
            RcColorType.Grey => Color.gray,
            _ => Color.white
        };
    }

    void SaveBrushState()
    {
        SessionState.SetString("RcLevelEditor.BrushType",  brushTileType?.name ?? "");
        SessionState.SetString("RcLevelEditor.BrushColor", brushColor.ToString());
    }

    void RestoreBrushState()
    {
        string typeName  = SessionState.GetString("RcLevelEditor.BrushType",  "");
        string colorName = SessionState.GetString("RcLevelEditor.BrushColor", "");

        brushTileType = allTileTypes.FirstOrDefault(t => t.name == typeName);
        brushColor = Enum.TryParse<RcColorType>(colorName, out var c) ? c : RcColorType.None;
    }
}

[InitializeOnLoad]
static class RcLevelEditorAutoSelect
{
    static RcLevelEditorAutoSelect()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        var proxy = UnityEngine.Object.FindFirstObjectByType<RcLevelEditorProxy>();
        if (proxy != null)
            Selection.activeGameObject = proxy.gameObject;
    }
}
