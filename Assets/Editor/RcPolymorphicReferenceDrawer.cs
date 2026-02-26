#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// RcPolymorphicReferenceAttribute가 붙은 [SerializeReference] 필드에
/// 타입 선택 드롭다운을 제공하는 PropertyDrawer.
/// TweenAnimator의 GenericMenu 패턴을 재사용 가능한 Drawer로 추출한 것이다.
[CustomPropertyDrawer(typeof(RcPolymorphicReferenceAttribute))]
public class RcPolymorphicReferenceDrawer : PropertyDrawer
{
    private const float ButtonWidth = 72f;
    private const float ButtonMargin = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (RcPolymorphicReferenceAttribute)attribute;
        var currentValue = property.managedReferenceValue;
        var typeName = GetDisplayTypeName(property);

        // 헤더 한 줄: [타입명] 레이블 + Type▾ 버튼
        var headerRect = new Rect(position.x, position.y, position.width - ButtonWidth - ButtonMargin, EditorGUIUtility.singleLineHeight);
        var buttonRect = new Rect(position.xMax - ButtonWidth, position.y, ButtonWidth, EditorGUIUtility.singleLineHeight);

        // 타입명을 레이블에 포함해 PropertyField 렌더링 (자식 필드 포함)
        var displayLabel = new GUIContent(label.text, label.tooltip);
        if (currentValue != null)
            displayLabel.text = $"{label.text}  <{typeName}>";

        // 버튼을 먼저 그려야 PropertyField foldout보다 클릭 이벤트 우선 획득
        if (GUI.Button(buttonRect, "Type ▾", EditorStyles.miniButton))
            ShowTypeMenu(property, attr.BaseType);

        EditorGUI.PropertyField(position, property, displayLabel, true);
    }

    private void ShowTypeMenu(SerializedProperty property, Type baseType)
    {
        var menu = new GenericMenu();
        var currentType = property.managedReferenceValue?.GetType();

        // null = 빈 슬롯
        menu.AddItem(new GUIContent("(empty)"), currentType == null, () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        menu.AddSeparator("");

        // baseType 자신 + 모든 비추상 파생 클래스
        var types = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract)
            .Prepend(baseType)
            .Where(t => !t.IsAbstract)
            .OrderBy(t => t.Name);

        foreach (var type in types)
        {
            var t = type;
            var name = FormatTypeName(type);
            var isCurrent = currentType == type;

            menu.AddItem(new GUIContent(name), isCurrent, () =>
            {
                var prev = property.managedReferenceValue;
                var next = (RcTileData)Activator.CreateInstance(t);

                // 타입 변경 시 공통 베이스 필드 보존
                if (prev is RcTileData prevTile)
                {
                    next.TileType = prevTile.TileType;
                    next.bCanEnter = prevTile.bCanEnter;
                }

                property.managedReferenceValue = next;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private static string GetDisplayTypeName(SerializedProperty property)
    {
        var fullName = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullName)) return "null";
        return FormatTypeName(fullName.Split(' ').Last());
    }

    private static string FormatTypeName(Type type) => FormatTypeName(type.Name);

    private static string FormatTypeName(string name)
    {
        // "RcColorTileData" → "Color"
        var result = name.Replace("Rc", "").Replace("TileData", "").Replace("Data", "");
        return string.IsNullOrEmpty(result) ? "Normal" : result;
    }
}
#endif
