using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Rolice.Editor;

[CustomEditor(typeof(RcDataTableSOBase), true)]
public class RcDataTableAutoInspector : Editor
{
    private ReorderableList    _list;
    private RcTableColumnDef[] _columns;

    private void OnEnable()
    {
        _columns = BuildColumns(target.GetType());
        var rowsProp = serializedObject.FindProperty("rows");
        if (rowsProp == null) return;

        _list = new ReorderableList(serializedObject, rowsProp,
            draggable: false, displayHeader: true,
            displayAddButton: false, displayRemoveButton: false);
        _list.drawHeaderCallback  = DrawHeader;
        _list.drawElementCallback = DrawElement;
        _list.elementHeight       = EditorGUIUtility.singleLineHeight + 2f;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (_list != null)
            _list.DoLayoutList();

        EditorGUILayout.Space(4);

        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
        if (GUILayout.Button("Open in Table Editor", GUILayout.Height(26)))
            RcDataTableAutoEditorWindow.OpenWith((ScriptableObject)target);
        GUI.backgroundColor = prev;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader(Rect rect)
    {
        float x = rect.x;
        foreach (var col in _columns)
        {
            EditorGUI.LabelField(new Rect(x, rect.y, col.Width, rect.height),
                col.Header, EditorStyles.boldLabel);
            x += col.Width;
        }
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var row = _list.serializedProperty.GetArrayElementAtIndex(index);
        float x = rect.x;
        float y = rect.y + 1f;
        float h = EditorGUIUtility.singleLineHeight;

        using (new EditorGUI.DisabledGroupScope(true))
        {
            foreach (var col in _columns)
            {
                var prop = row.FindPropertyRelative(col.PropertyName);
                if (prop != null)
                    EditorGUI.PropertyField(new Rect(x, y, col.Width, h), prop, GUIContent.none);
                x += col.Width;
            }
        }
    }

    private static RcTableColumnDef[] BuildColumns(Type tableType)
    {
        var rowType = GetRowType(tableType);
        if (rowType == null) return Array.Empty<RcTableColumnDef>();

        var fields = rowType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var defs   = new RcTableColumnDef[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            var f    = fields[i];
            var attr = f.GetCustomAttribute<RcColumnAttribute>();
            defs[i]  = new RcTableColumnDef
            {
                PropertyName = f.Name,
                Header       = attr?.Header ?? ObjectNames.NicifyVariableName(f.Name),
                Width        = attr?.Width  ?? 150f,
            };
        }
        return defs;
    }

    private static Type GetRowType(Type tableType)
    {
        for (var t = tableType; t != null && t != typeof(object); t = t.BaseType)
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RcDataTableSO<>))
                return t.GetGenericArguments()[0];
        return null;
    }
}
