using System.Collections.Generic;
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

    // 스케일 생성 전용
    private int _scaledMinGrid = 3;
    private int _scaledMaxGrid = 8;

    // 결과 리포트
    private struct StageReport
    {
        public int    StageNumber;
        public bool   Ok;
        public string Error;      // 첫 번째 에러 메시지 (Ok=false 일 때)
        public int    SolveRate;
    }

    private List<StageReport> _lastReport;
    private Vector2           _reportScroll;
    private bool              _showReport = true;

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
        _genParams.Shape  = (RcLevelAutoGenerator.ShapePreset)EditorGUILayout.EnumPopup("Shape",  _genParams.Shape);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Size", GUILayout.Width(60));
        _genParams.Width  = Mathf.Clamp(EditorGUILayout.IntField(_genParams.Width,  GUILayout.Width(40)), 3, 12);
        EditorGUILayout.LabelField("×", GUILayout.Width(14));
        _genParams.Height = Mathf.Clamp(EditorGUILayout.IntField(_genParams.Height, GUILayout.Width(40)), 3, 12);
        EditorGUILayout.EndHorizontal();

        _genParams.ColorCount     = EditorGUILayout.IntSlider("Colors",     _genParams.ColorCount,     2, 6);
        _genParams.FillRatio      = EditorGUILayout.Slider("Fill Ratio",    _genParams.FillRatio,      0.3f, 1f);
        _genParams.TurnMultiplier = EditorGUILayout.Slider("Turn Mult",     _genParams.TurnMultiplier, 1f, 3f);

        if (_genParams.Preset == RcLevelAutoGenerator.Preset.Hybrid)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Hybrid Settings", EditorStyles.miniLabel);
            _genParams.CriticalSegmentLength = EditorGUILayout.IntSlider("Segment Length", _genParams.CriticalSegmentLength, 3, 8);

            int tiles      = Mathf.RoundToInt(_genParams.Width * _genParams.Height * _genParams.FillRatio);
            int maxSegment = Mathf.Max(1, tiles / (_genParams.CriticalSegmentLength * 2));
            _genParams.CriticalSegmentCount = EditorGUILayout.IntSlider(
                $"Critical Segments (max {maxSegment})", _genParams.CriticalSegmentCount, 1, maxSegment);
        }

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

        // ── 버튼 영역 ─────────────────────────────────────────────
        var prevBg = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
        if (GUILayout.Button($"Generate  {_fromStage} ~ {_toStage}  ({_toStage - _fromStage + 1}개)", GUILayout.Height(36)))
            RunBatch(GetStageRange(_fromStage, _toStage));

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Scaled Generate (1 ~ 100)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Grid Min", GUILayout.Width(60));
        _scaledMinGrid = Mathf.Clamp(EditorGUILayout.IntField(_scaledMinGrid, GUILayout.Width(40)), 2, 12);
        EditorGUILayout.LabelField("Max", GUILayout.Width(30));
        _scaledMaxGrid = Mathf.Clamp(EditorGUILayout.IntField(_scaledMaxGrid, GUILayout.Width(40)), _scaledMinGrid, 12);
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
        if (GUILayout.Button("Generate 1 ~ 100  (난이도 자동 스케일 + 랜덤 패턴)", GUILayout.Height(36)))
            RunScaledBatch();

        // 실패 스테이지가 있으면 재생성 버튼 표시
        var failedStages = GetFailedStages();
        if (failedStages != null && failedStages.Count > 0)
        {
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.3f);
            if (GUILayout.Button($"Re-generate  실패 {failedStages.Count}개", GUILayout.Height(30)))
                RunBatch(failedStages);
        }

        GUI.backgroundColor = prevBg;

        // ── 리포트 ────────────────────────────────────────────────
        DrawReport();
    }

    // ──────────────────────────────────────────────────────────────
    private void RunBatch(List<int> stageNumbers)
    {
        if (!Directory.Exists(_outputFolder))
            Directory.CreateDirectory(_outputFolder);

        int count   = stageNumbers.Count;
        int created = 0;

        // Database 배열 크기 확보
        int maxStage = stageNumbers.Max();
        if (_database.Stages == null || _database.Stages.Length < maxStage)
        {
            var expanded = new RcLevelDataSO[maxStage];
            if (_database.Stages != null)
                _database.Stages.CopyTo(expanded, 0);
            _database.Stages = expanded;
        }

        if (_lastReport == null) _lastReport = new List<StageReport>();

        // 이번 배치 대상 스테이지의 기존 리포트 항목을 제거 후 재삽입
        _lastReport.RemoveAll(r => stageNumbers.Contains(r.StageNumber));

        try
        {
            for (int i = 0; i < count; i++)
            {
                int stageNumber = stageNumbers[i];

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
                ld.StageInfo.StageNumber = stageNumber;
                ld.StageInfo.DisplayName = $"STAGE {stageNumber}";

                EditorUtility.SetDirty(ld);
                _database.Stages[stageNumber - 1] = ld;

                // 검증
                var validation = RcLevelValidator.Validate(ld);
                _lastReport.Add(new StageReport
                {
                    StageNumber = stageNumber,
                    Ok          = validation.IsValid,
                    Error       = validation.Errors.Count > 0 ? validation.Errors[0] : "",
                    SolveRate   = validation.SolveRate,
                });

                created++;
            }

            _lastReport.Sort((a, b) => a.StageNumber.CompareTo(b.StageNumber));

            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int okCount   = _lastReport.Count(r => r.Ok);
            int failCount = _lastReport.Count(r => !r.Ok);
            Debug.Log($"[BatchGenerator] {created}개 생성. OK {okCount} / Fail {failCount}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            Repaint();
        }
    }

    // ──────────────────────────────────────────────────────────────
    private void RunScaledBatch()
    {
        var stageNumbers = GetStageRange(1, 100);
        var scaledRng    = new System.Random();

        if (!Directory.Exists(_outputFolder))
            Directory.CreateDirectory(_outputFolder);

        int maxStage = 100;
        if (_database.Stages == null || _database.Stages.Length < maxStage)
        {
            var expanded = new RcLevelDataSO[maxStage];
            if (_database.Stages != null)
                _database.Stages.CopyTo(expanded, 0);
            _database.Stages = expanded;
        }

        if (_lastReport == null) _lastReport = new List<StageReport>();
        _lastReport.Clear();

        try
        {
            for (int i = 0; i < stageNumbers.Count; i++)
            {
                int stageNumber = stageNumbers[i];

                EditorUtility.DisplayProgressBar(
                    "Scaled Batch Generator",
                    $"생성 중: Stage {stageNumber}  (난이도 스케일)",
                    (float)i / stageNumbers.Count);

                string path = $"{_outputFolder}/Stage_{stageNumber:D3}.asset";

                var ld = AssetDatabase.LoadAssetAtPath<RcLevelDataSO>(path);
                if (ld == null)
                {
                    ld = CreateInstance<RcLevelDataSO>();
                    AssetDatabase.CreateAsset(ld, path);
                }

                var p = RcLevelAutoGenerator.BuildScaledParams(stageNumber, scaledRng, _scaledMinGrid, _scaledMaxGrid);
                RcLevelAutoGenerator.Generate(ld, p, _colorTileType);
                ld.StageInfo.StageNumber = stageNumber;
                ld.StageInfo.DisplayName = $"STAGE {stageNumber}";

                EditorUtility.SetDirty(ld);
                _database.Stages[stageNumber - 1] = ld;

                var validation = RcLevelValidator.Validate(ld);
                _lastReport.Add(new StageReport
                {
                    StageNumber = stageNumber,
                    Ok          = validation.IsValid,
                    Error       = validation.Errors.Count > 0 ? validation.Errors[0] : "",
                    SolveRate   = validation.SolveRate,
                });
            }

            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int okCount   = _lastReport.Count(r => r.Ok);
            int failCount = _lastReport.Count(r => !r.Ok);
            Debug.Log($"[ScaledBatch] 100개 생성. OK {okCount} / Fail {failCount}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            Repaint();
        }
    }

    // ──────────────────────────────────────────────────────────────
    private void DrawReport()
    {
        if (_lastReport == null || _lastReport.Count == 0) return;

        EditorGUILayout.Space(8);

        int okCount   = _lastReport.Count(r => r.Ok);
        int failCount = _lastReport.Count(r => !r.Ok);

        _showReport = EditorGUILayout.Foldout(_showReport,
            $"결과:  OK {okCount}  /  Fail {failCount}  (총 {_lastReport.Count}개)", true);

        if (!_showReport) return;

        _reportScroll = EditorGUILayout.BeginScrollView(_reportScroll, GUILayout.MaxHeight(200));

        var prevBg = GUI.backgroundColor;
        foreach (var report in _lastReport)
        {
            GUI.backgroundColor = report.Ok
                ? new Color(0.3f, 0.7f, 0.3f)
                : new Color(0.8f, 0.3f, 0.3f);

            string solveText = report.SolveRate >= 0 ? $"  Solve {report.SolveRate}%" : "";
            string label     = report.Ok
                ? $"Stage {report.StageNumber:D3}  ✓{solveText}"
                : $"Stage {report.StageNumber:D3}  ✗  {report.Error}";

            EditorGUILayout.LabelField(label, EditorStyles.helpBox);
        }

        GUI.backgroundColor = prevBg;
        EditorGUILayout.EndScrollView();
    }

    // ──────────────────────────────────────────────────────────────
    private List<int> GetStageRange(int from, int to)
    {
        var list = new List<int>(to - from + 1);
        for (int i = from; i <= to; i++) list.Add(i);
        return list;
    }

    private List<int> GetFailedStages()
    {
        if (_lastReport == null) return null;
        return _lastReport.Where(r => !r.Ok).Select(r => r.StageNumber).ToList();
    }

    static T[] LoadAllAssets<T>() where T : ScriptableObject =>
        AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(a => a != null)
            .ToArray();
}
