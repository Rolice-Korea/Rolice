using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rolice;

public class RcFaceDataTableEditorWindow : EditorWindow
{
    [SerializeField] private RcFaceDataTable _target;

    private SerializedObject _so;
    private int              _selectedSkinIndex;
    private Vector2          _scrollPos;

    private GUIStyle _rowStyleEven;
    private GUIStyle _rowStyleOdd;

    private const float ColColorType = 105f;
    private const float ColMaterial  = 168f;
    private const float ColEffect    = 140f;
    private const float ColDel       = 22f;

    // ------------------------------------------------------------------ open

    [MenuItem("Tools/Rolice/Face Data Table Editor")]
    public static void OpenWindow() =>
        GetWindow<RcFaceDataTableEditorWindow>("Face Data Table");

    public static void OpenWith(RcFaceDataTable table)
    {
        var win = GetWindow<RcFaceDataTableEditorWindow>("Face Data Table");
        win.BindTarget(table);
        win.Focus();
    }

    // ------------------------------------------------------------------ lifecycle

    private void OnEnable()
    {
        RebuildSO();
        _rowStyleEven = MakeRowStyle(new Color(0.22f, 0.22f, 0.22f));
        _rowStyleOdd  = MakeRowStyle(new Color(0.26f, 0.26f, 0.26f));
    }

    private void OnDisable()
    {
        if (_rowStyleEven?.normal.background != null) DestroyImmediate(_rowStyleEven.normal.background);
        if (_rowStyleOdd?.normal.background  != null) DestroyImmediate(_rowStyleOdd.normal.background);
    }

    private static GUIStyle MakeRowStyle(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return new GUIStyle { normal = { background = tex } };
    }

    private void BindTarget(RcFaceDataTable table)
    {
        _target            = table;
        _selectedSkinIndex = 0;
        RebuildSO();
        Repaint();
    }

    private void RebuildSO() =>
        _so = _target != null ? new SerializedObject(_target) : null;

    private SerializedObject SO
    {
        get
        {
            if (_so == null && _target != null) RebuildSO();
            return _so;
        }
    }

    // ------------------------------------------------------------------ OnGUI

    private void OnGUI()
    {
        DrawToolbar();

        if (_target == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("FaceDataTable asset을 선택하거나, asset inspector에서 'Open in Table Editor'를 클릭하세요.", MessageType.Info);
            return;
        }

        SO.Update();

        var skinBundlesProp = SO.FindProperty("skinBundles");

        EditorGUILayout.Space(4);
        DrawSkinTabs(skinBundlesProp);
        EditorGUILayout.Space(6);

        if (skinBundlesProp.arraySize > 0)
        {
            _selectedSkinIndex = Mathf.Clamp(_selectedSkinIndex, 0, skinBundlesProp.arraySize - 1);
            DrawColorTable(skinBundlesProp.GetArrayElementAtIndex(_selectedSkinIndex));
        }
        else
        {
            EditorGUILayout.HelpBox("Skin이 없습니다. '+ Add Skin'으로 추가하세요.", MessageType.Info);
        }

        SO.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ toolbar

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("Face Data Table", EditorStyles.boldLabel, GUILayout.Width(128));

        EditorGUI.BeginChangeCheck();
        var picked = (RcFaceDataTable)EditorGUILayout.ObjectField(
            _target, typeof(RcFaceDataTable), false);
        if (EditorGUI.EndChangeCheck())
            BindTarget(picked);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(48)))
        {
            if (_target != null)
            {
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ skin tabs

    private void DrawSkinTabs(SerializedProperty skinBundlesProp)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skin", GUILayout.Width(34));

        var prevBg = GUI.backgroundColor;
        int deleteAt = -1;

        for (int i = 0; i < skinBundlesProp.arraySize; i++)
        {
            var bundle      = skinBundlesProp.GetArrayElementAtIndex(i);
            var skinTypeProp = bundle.FindPropertyRelative("SkinType");
            string label    = ((RcFaceSkinType)skinTypeProp.enumValueIndex).ToString();

            GUI.backgroundColor = _selectedSkinIndex == i ? new Color(0.45f, 0.75f, 1f) : prevBg;
            if (GUILayout.Button(label, GUILayout.Height(22), GUILayout.ExpandWidth(false)))
                _selectedSkinIndex = i;

            GUI.backgroundColor = new Color(0.9f, 0.45f, 0.45f);
            if (GUILayout.Button("✕", GUILayout.Width(18), GUILayout.Height(22)))
                deleteAt = i;

            GUI.backgroundColor = prevBg;
            GUILayout.Space(4);
        }

        if (deleteAt >= 0)
        {
            skinBundlesProp.DeleteArrayElementAtIndex(deleteAt);
            _selectedSkinIndex = Mathf.Clamp(_selectedSkinIndex, 0, skinBundlesProp.arraySize - 1);
            SO.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
            return;
        }

        // + Add Skin (only enum values not yet used, excluding Max)
        var usedTypes = CollectUsedSkinTypes(skinBundlesProp);
        bool canAdd = TryGetNextFreeSkinType(usedTypes, out RcFaceSkinType nextType);

        EditorGUI.BeginDisabledGroup(!canAdd);
        if (GUILayout.Button("+ Add Skin", GUILayout.Height(22), GUILayout.ExpandWidth(false)) && canAdd)
        {
            int idx = skinBundlesProp.arraySize;
            skinBundlesProp.InsertArrayElementAtIndex(idx);
            var newBundle = skinBundlesProp.GetArrayElementAtIndex(idx);
            newBundle.FindPropertyRelative("SkinType").enumValueIndex = (int)nextType;
            newBundle.FindPropertyRelative("Colors").arraySize        = 0;
            _selectedSkinIndex = idx;
        }
        EditorGUI.EndDisabledGroup();

        GUI.backgroundColor = prevBg;
        EditorGUILayout.EndHorizontal();
    }

    private static HashSet<RcFaceSkinType> CollectUsedSkinTypes(SerializedProperty skinBundlesProp)
    {
        var set = new HashSet<RcFaceSkinType>();
        for (int i = 0; i < skinBundlesProp.arraySize; i++)
            set.Add((RcFaceSkinType)skinBundlesProp
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("SkinType").enumValueIndex);
        return set;
    }

    private static bool TryGetNextFreeSkinType(HashSet<RcFaceSkinType> used, out RcFaceSkinType result)
    {
        foreach (RcFaceSkinType t in System.Enum.GetValues(typeof(RcFaceSkinType)))
        {
            if (t == RcFaceSkinType.Max) continue;
            if (!used.Contains(t)) { result = t; return true; }
        }
        result = RcFaceSkinType.Default;
        return false;
    }

    // ------------------------------------------------------------------ color table

    private void DrawColorTable(SerializedProperty bundleProp)
    {
        var colorsProp = bundleProp.FindPropertyRelative("Colors");

        DrawTableHeader();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        int deleteAt = -1;
        for (int i = 0; i < colorsProp.arraySize; i++)
        {
            var entry = colorsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal(i % 2 == 0 ? _rowStyleEven : _rowStyleOdd);

            EditorGUILayout.PropertyField(
                entry.FindPropertyRelative("ColorType"),    GUIContent.none, GUILayout.Width(ColColorType));
            EditorGUILayout.PropertyField(
                entry.FindPropertyRelative("FaceMaterial"), GUIContent.none, GUILayout.Width(ColMaterial));
            EditorGUILayout.PropertyField(
                entry.FindPropertyRelative("TileMaterial"), GUIContent.none, GUILayout.Width(ColMaterial));
            EditorGUILayout.PropertyField(
                entry.FindPropertyRelative("MatchEffect"),  GUIContent.none, GUILayout.Width(ColEffect));

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 0.45f, 0.45f);
            if (GUILayout.Button("✕", GUILayout.Width(ColDel), GUILayout.Height(18)))
                deleteAt = i;
            GUI.backgroundColor = prevBg;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (deleteAt >= 0)
            colorsProp.DeleteArrayElementAtIndex(deleteAt);

        EditorGUILayout.Space(4);

        if (GUILayout.Button("+ Add Color Row", GUILayout.Height(24)))
        {
            int idx = colorsProp.arraySize;
            colorsProp.InsertArrayElementAtIndex(idx);
            var row = colorsProp.GetArrayElementAtIndex(idx);
            row.FindPropertyRelative("ColorType").enumValueIndex        = 0;
            row.FindPropertyRelative("FaceMaterial").objectReferenceValue = null;
            row.FindPropertyRelative("TileMaterial").objectReferenceValue = null;
            row.FindPropertyRelative("MatchEffect").objectReferenceValue  = null;
        }
    }

    private void DrawTableHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("ColorType",    EditorStyles.boldLabel, GUILayout.Width(ColColorType));
        GUILayout.Label("FaceMaterial", EditorStyles.boldLabel, GUILayout.Width(ColMaterial));
        GUILayout.Label("TileMaterial", EditorStyles.boldLabel, GUILayout.Width(ColMaterial));
        GUILayout.Label("MatchEffect",  EditorStyles.boldLabel, GUILayout.Width(ColEffect));
        GUILayout.Label("",                                      GUILayout.Width(ColDel));
        EditorGUILayout.EndHorizontal();
    }

}
