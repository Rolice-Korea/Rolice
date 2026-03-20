using System.IO;
using System.Linq;
using Rolice.Data;
using UnityEditor;
using UnityEngine;

public class RcBatchLevelGeneratorWindow : EditorWindow
{
    [MenuItem("Rolice/Batch Level Generator")]
    public static void Open() => GetWindow<RcBatchLevelGeneratorWindow>("Batch Level Generator");

    private RcStageDatabaseSO _database;
    private string            _outputFolder = "Assets/Data/Stages";
    private int               _fromStage    = 1;
    private int               _toStage      = 25;

    private RcLevelAutoGenerator.GenParams _genParams = new();
    private RcTileTypeSO[]                 _allTileTypes;
    private RcTileTypeSO                   _colorTileType;

    private void OnEnable()
    {
        _allTileTypes  = LoadAllAssets<RcTileTypeSO>();
        _colorTileType = _allTileTypes.FirstOrDefault(t => t != null && t.bHasColor && !t.bHasTeleport && !t.bHasStone);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Batch Level Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        _database = (RcStageDatabaseSO)EditorGUILayout.ObjectField("Stage Database", _database, typeof(RcStageDatabaseSO), false);

        EditorGUILayout.BeginHorizontal();
        _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);
        if (GUILayout.Button("...", GUILayout.Width(28)))
        {
            string picked = EditorUtility.OpenFolderPanel("Select Output Folder", _outputFolder, "");
            if (!string.IsNullOrEmpty(picked))
                _outputFolder = "Assets" + picked.Substring(Application.dataPath.Length);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Stage Range", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("From", GUILayout.Width(40));
        _fromStage = Mathf.Max(1, EditorGUILayout.IntField(_fromStage, GUILayout.Width(50)));
        EditorGUILayout.LabelField("To", GUILayout.Width(20));
        _toStage = Mathf.Max(_fromStage, EditorGUILayout.IntField(_toStage, GUILayout.Width(50)));
        EditorGUILayout.LabelField($"({_toStage - _fromStage + 1}개)", GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Gen Params", EditorStyles.boldLabel);

        _genParams.Preset = (RcLevelAutoGenerator.Preset)EditorGUILayout.EnumPopup("Preset", _genParams.Preset);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Size", GUILayout.Width(60));
        _genParams.Width  = Mathf.Clamp(EditorGUILayout.IntField(_genParams.Width,  GUILayout.Width(40)), 3, 12);
        EditorGUILayout.LabelField("×", GUILayout.Width(14));
        _genParams.Height = Mathf.Clamp(EditorGUILayout.IntField(_genParams.Height, GUILayout.Width(40)), 3, 12);
        EditorGUILayout.EndHorizontal();

        _genParams.ColorCount     = EditorGUILayout.IntSlider("Colors",     _genParams.ColorCount,     2, 6);
        _genParams.FillRatio      = EditorGUILayout.Slider("Fill Ratio",    _genParams.FillRatio,      0.3f, 1f);
        _genParams.TurnMultiplier = EditorGUILayout.Slider("Turn Mult",     _genParams.TurnMultiplier, 1f, 3f);

        EditorGUILayout.Space(8);

        if (_colorTileType == null)
        {
            EditorGUILayout.HelpBox("Color TileType SO를 찾을 수 없습니다. (bHasColor=true 인 TileTypeSO 필요)", MessageType.Error);
            return;
        }

        if (_database == null)
        {
            EditorGUILayout.HelpBox("Stage Database를 지정해주세요.", MessageType.Warning);
            return;
        }

        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
        if (GUILayout.Button($"Generate  {_fromStage} ~ {_toStage}  ({_toStage - _fromStage + 1}개)", GUILayout.Height(36)))
            RunBatch();
        GUI.backgroundColor = prev;
    }

    private void RunBatch()
    {
        if (!Directory.Exists(_outputFolder))
            Directory.CreateDirectory(_outputFolder);

        int count   = _toStage - _fromStage + 1;
        int created = 0;

        // Database 배열 크기 확보
        int required = _toStage;
        if (_database.Stages == null || _database.Stages.Length < required)
        {
            var expanded = new RcLevelDataSO[required];
            if (_database.Stages != null)
                _database.Stages.CopyTo(expanded, 0);
            _database.Stages = expanded;
        }

        try
        {
            for (int i = 0; i < count; i++)
            {
                int stageNumber = _fromStage + i;

                EditorUtility.DisplayProgressBar(
                    "Batch Level Generator",
                    $"생성 중: Stage {stageNumber}",
                    (float)i / count);

                string path = $"{_outputFolder}/Stage_{stageNumber:D3}.asset";

                var ld = AssetDatabase.LoadAssetAtPath<RcLevelDataSO>(path);
                if (ld == null)
                {
                    ld = CreateInstance<RcLevelDataSO>();
                    AssetDatabase.CreateAsset(ld, path);
                }

                RcLevelAutoGenerator.Generate(ld, _genParams, _colorTileType);
                ld.StageInfo.StageNumber  = stageNumber;
                ld.StageInfo.DisplayName  = $"STAGE {stageNumber}";

                EditorUtility.SetDirty(ld);
                _database.Stages[stageNumber - 1] = ld;
                created++;
            }

            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[BatchGenerator] {created}개 생성 완료. ({_fromStage}~{_toStage})");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static T[] LoadAllAssets<T>() where T : ScriptableObject =>
        AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(a => a != null)
            .ToArray();
}
