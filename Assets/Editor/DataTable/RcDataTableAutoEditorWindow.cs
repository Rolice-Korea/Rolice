using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rolice.Editor
{

public struct RcTableColumnDef
{
    public string PropertyName;
    public string Header;
    public float  Width;
    public bool   IsManagedReference;  // [SerializeReference] 필드 여부
}

public class RcDataTableAutoEditorWindow : EditorWindow
{
    private ScriptableObject   _target;
    private SerializedObject   _so;
    private RcTableColumnDef[] _columns;
    private ReorderableList    _list;
    private Vector2            _scrollPos;

    private const float DRAG_HANDLE_W = 15f;

    // ------------------------------------------------------------------ open

    [MenuItem("Tools/Rolice/Data Table Editor")]
    public static void OpenWindow() =>
        CreateWindow<RcDataTableAutoEditorWindow>("Data Table Editor");

    public static void OpenWith(ScriptableObject table)
    {
        if (table == null) return;

        foreach (var w in Resources.FindObjectsOfTypeAll<RcDataTableAutoEditorWindow>())
        {
            if (w._target == table) { w.Focus(); return; }
        }

        var win = CreateWindow<RcDataTableAutoEditorWindow>(table.GetType().Name);
        win.Bind(table);
    }

    // ------------------------------------------------------------------ lifecycle

    private void Bind(ScriptableObject table)
    {
        _target      = table;
        _so          = new SerializedObject(table);
        _columns     = BuildColumns(table.GetType());
        titleContent = new GUIContent(table.GetType().Name);

        var rowsProp = _so.FindProperty("rows");
        _list = new ReorderableList(_so, rowsProp,
            draggable: true, displayHeader: true,
            displayAddButton: true, displayRemoveButton: true);
        _list.drawHeaderCallback    = DrawListHeader;
        _list.drawElementCallback   = DrawListElement;
        _list.elementHeightCallback = GetElementHeight;

        Repaint();
    }

    // ------------------------------------------------------------------ header / element

    private void DrawListHeader(Rect rect)
    {
        float x = rect.x + DRAG_HANDLE_W;
        foreach (var col in _columns)
        {
            EditorGUI.LabelField(new Rect(x, rect.y, col.Width, rect.height),
                col.Header, EditorStyles.boldLabel);
            x += col.Width;
        }
    }

    private float GetElementHeight(int index)
    {
        if (_list.serializedProperty.arraySize <= index) return EditorGUIUtility.singleLineHeight + 2f;

        var row       = _list.serializedProperty.GetArrayElementAtIndex(index);
        float maxH    = EditorGUIUtility.singleLineHeight;

        foreach (var col in _columns)
        {
            if (!col.IsManagedReference) continue;
            var prop = row.FindPropertyRelative(col.PropertyName);
            if (prop == null) continue;
            float h = EditorGUI.GetPropertyHeight(prop, GUIContent.none, true);
            maxH = Mathf.Max(maxH, h);
        }

        return maxH + 2f;
    }

    private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var row = _list.serializedProperty.GetArrayElementAtIndex(index);
        float x = rect.x;
        float y = rect.y + 1f;
        float h = EditorGUIUtility.singleLineHeight;

        foreach (var col in _columns)
        {
            var prop = row.FindPropertyRelative(col.PropertyName);
            if (prop == null)
            {
                EditorGUI.LabelField(new Rect(x, y, col.Width, h), $"?{col.PropertyName}");
            }
            else if (col.IsManagedReference)
            {
                // [SerializeReference] — 타입 선택 + 자식 필드 포함 드로
                float propH = EditorGUI.GetPropertyHeight(prop, GUIContent.none, true);
                EditorGUI.PropertyField(new Rect(x, y, col.Width, propH), prop, GUIContent.none, true);
            }
            else
            {
                EditorGUI.PropertyField(new Rect(x, y, col.Width, h), prop, GUIContent.none);
            }
            x += col.Width;
        }
    }

    // ------------------------------------------------------------------ reflection

    private static RcTableColumnDef[] BuildColumns(Type tableType)
    {
        var rowType = GetRowType(tableType);
        if (rowType == null) return Array.Empty<RcTableColumnDef>();

        var fields = rowType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var defs   = new RcTableColumnDef[fields.Length];

        for (int i = 0; i < fields.Length; i++)
        {
            var f             = fields[i];
            var attr          = f.GetCustomAttribute<RcColumnAttribute>();
            bool isManagedRef = f.IsDefined(typeof(SerializeReference), false);

            defs[i] = new RcTableColumnDef
            {
                PropertyName       = f.Name,
                Header             = attr?.Header ?? ObjectNames.NicifyVariableName(f.Name),
                Width              = attr?.Width  ?? 150f,
                IsManagedReference = isManagedRef,
            };
        }
        return defs;
    }

    private static Type GetRowType(Type tableType)
    {
        for (var t = tableType; t != null && t != typeof(object); t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RcDataTableSO<>))
                return t.GetGenericArguments()[0];
        }
        return null;
    }

    // ------------------------------------------------------------------ OnGUI

    private void OnGUI()
    {
        DrawToolbar();

        if (_target == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Inspector에서 'Open in Table Editor'를 클릭하거나 위 피커에서 SO를 선택하세요.",
                MessageType.Info);
            return;
        }

        _so.Update();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        _list.DoLayoutList();
        EditorGUILayout.EndScrollView();

        _so.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ toolbar

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField(
            _target != null ? _target.GetType().Name : "Data Table",
            EditorStyles.boldLabel, GUILayout.Width(180));

        EditorGUI.BeginChangeCheck();
        var picked = (ScriptableObject)EditorGUILayout.ObjectField(
            _target, typeof(RcDataTableSOBase), false);
        if (EditorGUI.EndChangeCheck() && picked != null)
            Bind(picked);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(48)) && _target != null)
        {
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndHorizontal();
    }
}

} // namespace Rolice.Editor
