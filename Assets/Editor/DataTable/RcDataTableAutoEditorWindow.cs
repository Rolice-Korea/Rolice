using System;
using System.Collections.Generic;
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

        var   row  = _list.serializedProperty.GetArrayElementAtIndex(index);
        float maxH = EditorGUIUtility.singleLineHeight;

        foreach (var col in _columns)
        {
            if (!col.IsManagedReference) continue;
            var prop = row.FindPropertyRelative(col.PropertyName);
            if (prop == null) continue;
            maxH = Mathf.Max(maxH, GetManagedReferenceHeight(prop));
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
                DrawManagedReferenceField(new Rect(x, y, col.Width, rect.height), prop);
            }
            else
            {
                EditorGUI.PropertyField(new Rect(x, y, col.Width, h), prop, GUIContent.none);
            }
            x += col.Width;
        }
    }

    // ------------------------------------------------------------------ managed reference

    private static float GetManagedReferenceHeight(SerializedProperty prop)
    {
        float h = EditorGUIUtility.singleLineHeight + 2f; // 타입 선택 버튼
        if (prop.managedReferenceValue == null) return h;

        var child = prop.Copy();
        var end   = prop.GetEndProperty();
        if (child.Next(true))
        {
            while (!SerializedProperty.EqualContents(child, end))
            {
                h += EditorGUI.GetPropertyHeight(child, true) + 2f;
                if (!child.Next(false)) break;
            }
        }
        return h;
    }

    private static void DrawManagedReferenceField(Rect cellRect, SerializedProperty prop)
    {
        float lineH  = EditorGUIUtility.singleLineHeight;
        float btnW   = cellRect.width;
        var   curRef = prop.managedReferenceValue;
        string label = curRef != null ? curRef.GetType().Name : "None";

        // 타입 선택 드롭박스
        if (EditorGUI.DropdownButton(
            new Rect(cellRect.x, cellRect.y, btnW, lineH),
            new GUIContent(label), FocusType.Passive))
        {
            var fieldType = GetManagedReferenceFieldType(prop);
            var menu      = new GenericMenu();

            menu.AddItem(new GUIContent("None"), curRef == null, () =>
            {
                prop.managedReferenceValue = null;
                prop.serializedObject.ApplyModifiedProperties();
            });

            if (fieldType != null)
            {
                foreach (var t in GetConcreteTypes(fieldType))
                {
                    var  captured   = t;
                    bool isSelected = curRef?.GetType() == t;
                    menu.AddItem(new GUIContent(t.Name), isSelected, () =>
                    {
                        prop.managedReferenceValue = Activator.CreateInstance(captured);
                        prop.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            menu.ShowAsContext();
        }

        // 자식 필드 (타입이 설정된 경우)
        if (curRef == null) return;

        float y     = cellRect.y + lineH + 2f;
        var   child = prop.Copy();
        var   end   = prop.GetEndProperty();

        if (!child.Next(true)) return;
        while (!SerializedProperty.EqualContents(child, end))
        {
            float fieldH = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(
                new Rect(cellRect.x + 8f, y, cellRect.width - 8f, fieldH),
                child, true);
            y += fieldH + 2f;
            if (!child.Next(false)) break;
        }
    }

    private static Type GetManagedReferenceFieldType(SerializedProperty prop)
    {
        // "assemblyName typeName" 형식
        var parts = prop.managedReferenceFieldTypename?.Split(' ');
        if (parts == null || parts.Length < 2) return null;
        try { return Assembly.Load(parts[0])?.GetType(parts[1]); }
        catch { return null; }
    }

    private static IEnumerable<Type> GetConcreteTypes(Type baseType) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
            .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
            .OrderBy(t => t.Name);

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
