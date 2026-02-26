using UnityEngine;
using UnityEditor;

public class RcLevelTemplateCreator : EditorWindow
{
    private TemplateType selectedTemplate = TemplateType.Small5x5;
    private string levelName = "NewLevel";
    private RcTileTypeSO defaultTileType;
    
    private enum TemplateType
    {
        Empty,          // 빈 맵
        Small5x5,       // 5x5 기본 맵
        Medium7x7,      // 7x7 기본 맵
        Large10x10,     // 10x10 기본 맵
        CrossShape,     // 십자 모양
        LShape,         // L자 모양
        Tutorial        // 튜토리얼용 간단한 맵
    }
    
    [MenuItem("Tools/Rolice/Template Creator")]
    static void OpenWindow()
    {
        var window = GetWindow<RcLevelTemplateCreator>("Template Creator");
        window.minSize = new Vector2(350, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Level Template Creator", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "빠른 프로토타입을 위한 템플릿을 생성합니다.\n" +
            "생성 후 Level Editor에서 수정할 수 있습니다.",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("Template Type:", EditorStyles.miniBoldLabel);
        
        selectedTemplate = (TemplateType)EditorGUILayout.EnumPopup(selectedTemplate);
        
        GUILayout.Space(5);
        
        // 템플릿 설명
        DrawTemplateDescription(selectedTemplate);
        
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Level Name:", EditorStyles.miniBoldLabel);
        levelName = EditorGUILayout.TextField(levelName);
        EditorGUILayout.EndVertical();

        GUILayout.Space(5);

EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Default Tile Type:", EditorStyles.miniBoldLabel);
        defaultTileType = (RcTileTypeSO)EditorGUILayout.ObjectField(defaultTileType, typeof(RcTileTypeSO), false);
        EditorGUILayout.HelpBox("미설정 시 타일이 빈 상태로 생성됩니다. Level Editor에서 직접 할당하세요.", MessageType.None);
        EditorGUILayout.EndVertical();
        
        GUILayout.Space(10);
        
if (GUILayout.Button("Create Template", GUILayout.Height(40)))
        {
            CreateTemplate();
        }
    }
    
    void DrawTemplateDescription(TemplateType type)
    {
        string description = type switch
        {
            TemplateType.Empty => "완전히 빈 맵 (직접 디자인용)",
            TemplateType.Small5x5 => "5x5 기본 맵 (간단한 퍼즐)",
            TemplateType.Medium7x7 => "7x7 기본 맵 (일반적인 레벨)",
            TemplateType.Large10x10 => "10x10 기본 맵 (복잡한 퍼즐)",
            TemplateType.CrossShape => "십자 모양 레이아웃",
            TemplateType.LShape => "L자 모양 레이아웃",
            TemplateType.Tutorial => "튜토리얼용 간단한 맵 (3x3)",
            _ => "Unknown template"
        };
        
        EditorGUILayout.HelpBox(description, MessageType.None);
    }
    
    void CreateTemplate()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Level Template",
            levelName,
            "asset",
            "템플릿을 저장할 위치를 선택하세요"
        );
        
        if (string.IsNullOrEmpty(path))
            return;
        
        RcLevelDataSO level = selectedTemplate switch
        {
            TemplateType.Empty => CreateEmptyLevel(),
            TemplateType.Small5x5 => CreateSmall5x5(),
            TemplateType.Medium7x7 => CreateMedium7x7(),
            TemplateType.Large10x10 => CreateLarge10x10(),
            TemplateType.CrossShape => CreateCrossShape(),
            TemplateType.LShape => CreateLShape(),
            TemplateType.Tutorial => CreateTutorial(),
            _ => CreateEmptyLevel()
        };
        
        AssetDatabase.CreateAsset(level, path);
        AssetDatabase.SaveAssets();
        
        Selection.activeObject = level;
        
        EditorUtility.DisplayDialog(
            "Template Created",
            $"템플릿이 생성되었습니다!\n\n{path}\n\nLevel Editor에서 편집할 수 있습니다.",
            "OK"
        );
    }
    
RcLevelDataSO CreateEmptyLevel()
    {
        return CreateLevel(7, 7, (x, y) => null);
    }
    
    RcLevelDataSO CreateSmall5x5()
    {
        return CreateLevel(5, 5, (x, y) =>
        {
            return new RcTileData { TileType = defaultTileType, bCanEnter = true };
        });
    }

    RcLevelDataSO CreateMedium7x7()
    {
        return CreateLevel(7, 7, (x, y) =>
        {
            return new RcTileData { TileType = defaultTileType, bCanEnter = true };
        });
    }

    RcLevelDataSO CreateLarge10x10()
    {
        return CreateLevel(10, 10, (x, y) =>
        {
            return new RcTileData { TileType = defaultTileType, bCanEnter = true };
        });
    }

    RcLevelDataSO CreateCrossShape()
    {
        return CreateLevel(7, 7, (x, y) =>
        {
            // 십자 모양 (가운데 행과 열만 타일 배치)
            int centerX = 3;
            int centerY = 3;

            if (x == centerX || y == centerY)
                return new RcTileData { TileType = defaultTileType, bCanEnter = true };

            return null;
        });
    }

    RcLevelDataSO CreateLShape()
    {
        return CreateLevel(7, 7, (x, y) =>
        {
            // L자 모양 (왼쪽 열 + 아래 행)
            if (x == 0 || y == 0)
                return new RcTileData { TileType = defaultTileType, bCanEnter = true };

            return null;
        });
    }

    RcLevelDataSO CreateTutorial()
    {
        return CreateLevel(3, 3, (x, y) =>
        {
            return new RcTileData { TileType = defaultTileType, bCanEnter = true };
        });
    }
    
    RcLevelDataSO CreateLevel(int width, int height, System.Func<int, int, RcTileData> tileFactory)
    {
        RcLevelDataSO level = CreateInstance<RcLevelDataSO>();
        
        level.Width = width;
        level.Height = height;
        level.Tiles = new RcTileData[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                RcTileData tile = tileFactory(x, y);
                
                level.Tiles[index] = tile; // null = 빈 타일 (SerializeReference에서 유효)
            }
        }
        
        // 기본 게임 룰 설정
        level.Rules = new RcLevelRules
        {
            HasTurnLimit = true,
            MaxTurns = 15
        };
        
        return level;
    }
}