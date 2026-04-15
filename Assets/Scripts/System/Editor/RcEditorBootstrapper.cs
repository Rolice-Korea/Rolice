#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Rolice.System.Editor
{
    /// <summary>
    /// 에디터에서 어느 씬이든 플레이 버튼을 누르면 InitScene을 통해 부트스트랩 플로우를 거치도록 강제.
    /// ExitingEditMode: 현재 씬 경로 저장 → InitScene 오픈
    /// EnteredEditMode: 저장된 씬 복구
    /// </summary>
    [InitializeOnLoad]
    public static class RcEditorBootstrapper
    {
        private const string PrevSceneKey  = "RcEditor_PrevScene";
        private const string InitScenePath = "Assets/Scenes/InitScene.unity";

        static RcEditorBootstrapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string currentPath = SceneManager.GetActiveScene().path;
                EditorPrefs.SetString(PrevSceneKey, currentPath);

                // 이미 InitScene이면 전환 불필요
                if (currentPath != InitScenePath)
                    EditorSceneManager.OpenScene(InitScenePath);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                string prev = EditorPrefs.GetString(PrevSceneKey, string.Empty);
                if (!string.IsNullOrEmpty(prev) && prev != InitScenePath)
                    EditorSceneManager.OpenScene(prev);
            }
        }
    }
}
#endif
