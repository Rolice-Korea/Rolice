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
        _list.drawHeaderCallback    = DrawHeader;
        _list.drawElementCallback   = DrawElement;
        _list.elementHeightCallback = GetElementHeight;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // rows 제외한 헤더 필드 먼저 표시
        var iter = serializedObject.GetIterator();
        iter.NextVisible(true); // m_Script skip
        while (iter.NextVisible(false))
        {
            if (iter.name == "rows") continue;
            EditorGUILayout.PropertyField(iter, true);
        }

        EditorGUILayout.Space(4);

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

    private float GetElementHeight(int index)
    {
        if (_list.serializedProperty.arraySize <= index) return EditorGUIUtility.singleLineHeight + 2f;

        var row    = _list.serializedProperty.GetArrayElementAtIndex(index);
        float maxH = EditorGUIUtility.singleLineHeight;

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
                if (prop == null)
                {
                    x += col.Width;
                    continue;
                }

                if (col.IsManagedReference)
                {
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
    }

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
            defs[i]  = new RcTableColumnDef
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
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RcDataTableSO<>))
                return t.GetGenericArguments()[0];
        return null;
    }
}
