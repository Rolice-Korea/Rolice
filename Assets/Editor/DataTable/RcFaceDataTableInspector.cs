using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RcFaceDataTable))]
public class RcFaceDataTableInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var table           = (RcFaceDataTable)target;
        var skinBundlesProp = serializedObject.FindProperty("skinBundles");

        EditorGUILayout.LabelField("Face Data Table", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            skinBundlesProp.arraySize > 0
                ? $"{skinBundlesProp.arraySize} skin(s) registered"
                : "No skins",
            EditorStyles.miniLabel);

        EditorGUILayout.Space(10);

        var prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
        if (GUILayout.Button("Open in Table Editor", GUILayout.Height(30)))
            RcFaceDataTableEditorWindow.OpenWith(table);
        GUI.backgroundColor = prev;
    }
}
