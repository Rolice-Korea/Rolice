using UnityEditor;
using UnityEngine;

/// RcTileTypeSO Inspector 커스터마이징.
/// TileDataTemplate 필드를 타입 드롭다운으로만 표시하고 자식 필드(Color 등)는 숨긴다.
[CustomEditor(typeof(RcTileTypeSO))]
public class RcTileTypeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // TileDataTemplate 제외한 기본 필드 드로우
        DrawPropertiesExcluding(serializedObject, "TileDataTemplate");

        // TileDataTemplate — 타입만 표시
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

        var prop        = serializedObject.FindProperty("TileDataTemplate");
        string typeName = prop.managedReferenceValue != null
            ? prop.managedReferenceValue.GetType().Name
            : "None";

        Rect row        = EditorGUILayout.GetControlRect();
        Rect labelRect  = new Rect(row.x, row.y, EditorGUIUtility.labelWidth, row.height);
        Rect buttonRect = new Rect(row.x + EditorGUIUtility.labelWidth, row.y,
                                   row.width - EditorGUIUtility.labelWidth, row.height);

        EditorGUI.LabelField(labelRect,
            new GUIContent("Tile Data Type", "배치 시 생성할 TileData 클래스. Color 등 실제 값은 에디터가 주입한다."));

        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(typeName), FocusType.Keyboard))
            ShowTypeDropdown(prop);

        serializedObject.ApplyModifiedProperties();
    }

    void ShowTypeDropdown(SerializedProperty prop)
    {
        var menu        = new GenericMenu();
        var currentType = prop.managedReferenceValue?.GetType();

        menu.AddItem(new GUIContent("None"), currentType == null, () =>
        {
            prop.managedReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        });

        // RcTileData 서브클래스를 자동 탐색 — 새 타입 추가해도 코드 수정 불필요
        foreach (var type in TypeCache.GetTypesDerivedFrom<RcTileData>())
        {
            if (type.IsAbstract) continue;

            var  capturedType = type;
            bool isActive     = currentType == capturedType;

            menu.AddItem(new GUIContent(type.Name), isActive, () =>
            {
                prop.managedReferenceValue = System.Activator.CreateInstance(capturedType);
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}
