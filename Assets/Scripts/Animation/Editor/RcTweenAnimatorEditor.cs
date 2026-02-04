#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RcTweenAnimator))]
public class RcTweenAnimatorEditor : Editor
{
    private SerializedProperty _sequencesProperty;
    private SerializedProperty _playOnEnableProperty;
    private SerializedProperty _onDisableBehaviorProperty;

    private void OnEnable()
    {
        _sequencesProperty = serializedObject.FindProperty("_sequences");
        _playOnEnableProperty = serializedObject.FindProperty("_playOnEnable");
        _onDisableBehaviorProperty = serializedObject.FindProperty("_onDisableBehaviorType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_playOnEnableProperty, new GUIContent("Play On Enable"));
        EditorGUILayout.PropertyField(_onDisableBehaviorProperty, new GUIContent("On Disable Behavior"));
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Sequences", EditorStyles.boldLabel);

        if (_sequencesProperty.arraySize == 0)
            EditorGUILayout.HelpBox("No sequences.", MessageType.Info);
        else
        {
            CheckDuplicateSequenceNames();
            for (int i = 0; i < _sequencesProperty.arraySize; i++)
                DrawSequenceElement(i);
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("+ Add Sequence", GUILayout.Height(30)))
            AddNewSequence();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);

            var animator = (RcTweenAnimator)target;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play")) animator.Play();
            if (GUILayout.Button("Stop")) animator.Stop();
            if (GUILayout.Button("Pause")) animator.Pause();
            if (GUILayout.Button("Resume")) animator.Resume();
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CheckDuplicateSequenceNames()
    {
        var names = Enumerable.Range(0, _sequencesProperty.arraySize)
            .Select(i => _sequencesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue)
            .ToList();

        var duplicates = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
            EditorGUILayout.HelpBox($"Duplicate names: {string.Join(", ", duplicates)}", MessageType.Warning);
    }

    private void DrawSequenceElement(int index)
    {
        var seq = _sequencesProperty.GetArrayElementAtIndex(index);
        var nameProp = seq.FindPropertyRelative("name");
        var animsProp = seq.FindPropertyRelative("animations");
        var loopProp = seq.FindPropertyRelative("loop");
        var loopCountProp = seq.FindPropertyRelative("loopCount");
        var loopTypeProp = seq.FindPropertyRelative("loopType");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"[{index}]", EditorStyles.boldLabel, GUILayout.Width(30));
        nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);

        if (GUILayout.Button("▾", GUILayout.Width(20)))
            ShowNamePresetMenu(nameProp);

        GUILayout.FlexibleSpace();

        GUI.enabled = index > 0;
        if (GUILayout.Button("▲", GUILayout.Width(25)))
        {
            _sequencesProperty.MoveArrayElement(index, index - 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        GUI.enabled = index < _sequencesProperty.arraySize - 1;
        if (GUILayout.Button("▼", GUILayout.Width(25)))
        {
            _sequencesProperty.MoveArrayElement(index, index + 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.enabled = true;

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("✕", GUILayout.Width(25)))
        {
            _sequencesProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(loopProp);
        if (loopProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(loopCountProp, new GUIContent("Count (-1 = ∞)"));
            EditorGUILayout.PropertyField(loopTypeProp, new GUIContent("Type"));
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

        if (animsProp.arraySize == 0)
            EditorGUILayout.HelpBox("Empty", MessageType.Info);
        else
        {
            for (int i = 0; i < animsProp.arraySize; i++)
                DrawAnimationElement(animsProp, i);
        }

        EditorGUILayout.Space(3);
        if (GUILayout.Button("+ Add Animation", GUILayout.Height(25)))
            ShowAddAnimationMenu(animsProp);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawAnimationElement(SerializedProperty animsProp, int index)
    {
        var element = animsProp.GetArrayElementAtIndex(index);

        if (element.managedReferenceValue == null)
        {
            EditorGUILayout.HelpBox($"[{index}] Null", MessageType.Warning);
            if (GUILayout.Button("Remove"))
                animsProp.DeleteArrayElementAtIndex(index);
            return;
        }

        var typeName = element.managedReferenceFullTypename
            .Split(' ').Last()
            .Split('.').Last()
            .Replace("Rc", "")
            .Replace("Config", "");

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"[{index}] {typeName}", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.FlexibleSpace();

        GUI.enabled = index > 0;
        if (GUILayout.Button("▲", GUILayout.Width(25)))
        {
            animsProp.MoveArrayElement(index, index - 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        GUI.enabled = index < animsProp.arraySize - 1;
        if (GUILayout.Button("▼", GUILayout.Width(25)))
        {
            animsProp.MoveArrayElement(index, index + 1);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.enabled = true;

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("✕", GUILayout.Width(25)))
        {
            animsProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        var iter = element.Copy();
        var end = iter.GetEndProperty();
        iter.NextVisible(true);
        while (iter.NextVisible(false))
        {
            if (SerializedProperty.EqualContents(iter, end)) break;
            EditorGUILayout.PropertyField(iter, true);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void AddNewSequence()
    {
        int idx = _sequencesProperty.arraySize;
        _sequencesProperty.InsertArrayElementAtIndex(idx);

        var seq = _sequencesProperty.GetArrayElementAtIndex(idx);
        seq.FindPropertyRelative("name").stringValue = "";
        seq.FindPropertyRelative("animations").ClearArray();
        seq.FindPropertyRelative("loop").boolValue = false;
        seq.FindPropertyRelative("loopCount").intValue = -1;
        seq.FindPropertyRelative("loopType").enumValueIndex = 0;

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowAddAnimationMenu(SerializedProperty animsProp)
    {
        var menu = new GenericMenu();
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && typeof(RcTweenConfig).IsAssignableFrom(t))
            .OrderBy(t => t.Name);

        foreach (var type in types)
        {
            var name = type.Name.Replace("Rc", "").Replace("Config", "");
            menu.AddItem(new GUIContent(name), false, () =>
            {
                int idx = animsProp.arraySize;
                animsProp.InsertArrayElementAtIndex(idx);
                animsProp.GetArrayElementAtIndex(idx).managedReferenceValue = Activator.CreateInstance(type);
                serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private void ShowNamePresetMenu(SerializedProperty nameProp)
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("UI/OnClick"), false, () => SetName(nameProp, "OnClick"));
        menu.AddItem(new GUIContent("UI/OnEnter"), false, () => SetName(nameProp, "OnEnter"));
        menu.AddItem(new GUIContent("UI/OnExit"), false, () => SetName(nameProp, "OnExit"));
        menu.AddItem(new GUIContent("UI/OnOpen"), false, () => SetName(nameProp, "OnOpen"));
        menu.AddItem(new GUIContent("UI/OnClose"), false, () => SetName(nameProp, "OnClose"));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("General/Default"), false, () => SetName(nameProp, "Default"));
        menu.AddItem(new GUIContent("General/Idle"), false, () => SetName(nameProp, "Idle"));
        menu.AddItem(new GUIContent("General/Start"), false, () => SetName(nameProp, "Start"));
        menu.AddItem(new GUIContent("General/End"), false, () => SetName(nameProp, "End"));
        menu.AddItem(new GUIContent("General/Loop"), false, () => SetName(nameProp, "Loop"));

        menu.ShowAsContext();
    }

    private void SetName(SerializedProperty prop, string name)
    {
        prop.stringValue = name;
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
