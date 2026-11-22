using Engine.UI;
using Rolice;
using Rolice.UI;
using UnityEngine;

public class RcGameBootstrap : MonoBehaviour
{
    private const string NormalTilePrefabResourcePath = "Tiles/color";

    private static readonly RcColorType[] RandomPoolColors =
    {
        RcColorType.Magenta,
        RcColorType.Yellow,
        RcColorType.Cyan,
    };

    [Header("Level Settings")]
    [SerializeField] private Transform tilesParent;
    [SerializeField] private RcDicePawn dicePawn;
    [SerializeField] private RcTileTypeSO normalTileType;

    [Header("Fallback (에디터 직접 실행용)")]
    [SerializeField] private RcLevelDataSO fallbackLevel;

    private RcLevelDataSO runtimeLevelData;
    private RcTileTypeSO runtimeTileType;

    private void Awake()
    {
        RcLevelDataSO levelData = GenerateRandomLevel();
        if (levelData != null)
        {
            GameLoad(levelData);
        }
        else
        {
            Debug.LogError("[Bootstrap] 랜덤 레벨 생성에 실패했습니다!");
        }
    }

    private void OnDestroy()
    {
        if (runtimeLevelData != null)
            Destroy(runtimeLevelData);

        if (runtimeTileType != null)
            Destroy(runtimeTileType);
    }

    private RcLevelDataSO GenerateRandomLevel()
    {
        int gridSize = RcGameContext.SelectedGridSize > 0
            ? RcGameContext.SelectedGridSize
            : RcGameContext.DefaultGridSize;

        RcTileTypeSO tileType = ResolveNormalTileType();
        if (tileType == null)
        {
            Debug.LogError("[Bootstrap] 기본 타일(NormalTileType)을 찾을 수 없습니다!");
            return null;
        }

        runtimeLevelData = ScriptableObject.CreateInstance<RcLevelDataSO>();
        runtimeLevelData.Width = gridSize;
        runtimeLevelData.Height = gridSize;

        int totalTiles = gridSize * gridSize;
        runtimeLevelData.Tiles = new RcTileData[totalTiles];

        for (int i = 0; i < totalTiles; i++)
        {
            RcColorType randomColor = RandomPoolColors[Random.Range(0, RandomPoolColors.Length)];
            runtimeLevelData.Tiles[i] = new RcTileData
            {
                TileType = tileType,
                colorType = randomColor
            };
        }

        runtimeLevelData.SpawnGridPosition = new Vector2Int(
            Random.Range(0, gridSize),
            Random.Range(0, gridSize)
        );

        runtimeLevelData.InitialDiceFaces = (RcColorType[])RcLevelDataSO.DefaultDiceFaces.Clone();
        runtimeLevelData.Rules = new RcLevelRules
        {
            HasTurnLimit = false
        };

        return runtimeLevelData;
    }

    private RcTileTypeSO ResolveNormalTileType()
    {
        if (normalTileType != null && normalTileType.Prefab != null)
            return normalTileType;

        if (fallbackLevel != null && fallbackLevel.Tiles != null)
        {
            foreach (var tile in fallbackLevel.Tiles)
            {
                if (tile != null && tile.TileType != null && tile.TileType.Prefab != null)
                    return tile.TileType;
            }
        }

        GameObject prefab = Resources.Load<GameObject>(NormalTilePrefabResourcePath);
        if (prefab == null)
            return null;

        runtimeTileType = ScriptableObject.CreateInstance<RcTileTypeSO>();
        runtimeTileType.name = "RuntimeNormalTileType";
        runtimeTileType.Prefab = prefab;
        runtimeTileType.bHasColor = true;
        return runtimeTileType;
    }

    private void GameLoad(RcLevelDataSO levelData)
    {
        LoadLevel(levelData);
        PlaceDice(levelData);
        InitializeDiceFaces(levelData);
    }

    private void PlaceDice(RcLevelDataSO levelData)
    {
        if (dicePawn == null) return;

        var spawn = levelData.SpawnGridPosition;
        if (spawn.x < 0 || spawn.y < 0) return;

        dicePawn.SetSpawnPosition(spawn);
    }

    private void Start()
    {
        RcUIManager.Instance.Open<RcGameHudPanel>();
        RcGameResultManager.Instance.Initialize();
    }

    private void LoadLevel(RcLevelDataSO levelData)
    {
        RcLevelLoadResult result = RcLevelManager.Instance.LoadLevel(levelData, tilesParent);

        if (!result.Success)
        {
            Debug.LogError($"[Bootstrap] 레벨 로드 실패: {result.ErrorMessage}");
            return;
        }

        InitializeGameRules(levelData);
    }

    private void InitializeDiceFaces(RcLevelDataSO levelData)
    {
        if (dicePawn == null) return;

        var faces = levelData.InitialDiceFaces;
        if (faces == null || faces.Length != 6) return;

        foreach (var face in faces)
        {
            if (face != RcColorType.None)
            {
                dicePawn.InitializeFaces(faces);
                return;
            }
        }

        dicePawn.RefreshVisuals();
    }

    private void InitializeGameRules(RcLevelDataSO levelData)
    {
        if (levelData == null || levelData.Rules == null)
        {
            Debug.LogWarning("[Bootstrap] LevelRules가 없습니다!");
            return;
        }

        RcGameRuleManager.Instance.Initialize(levelData.Rules);
    }
}
