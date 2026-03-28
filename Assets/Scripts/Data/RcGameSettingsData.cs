using System;
using System.IO;
using Rolice;
using UnityEngine;

namespace Rolice.Data
{
    [Serializable]
    public class RcGameSettingsData
    {
        public int ScreenMode = (int)RcScreenMode.Portrait;

        // --- 정적 API ---

        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "settings.json");

        private static RcGameSettingsData _current;
        public static RcGameSettingsData Current => _current ??= Load();

        public static void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(_current, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Settings] 저장 실패: {e.Message}");
            }
        }

        public static RcGameSettingsData Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return new RcGameSettingsData();

                var json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<RcGameSettingsData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Settings] 로드 실패: {e.Message}");
                return new RcGameSettingsData();
            }
        }

        // --- 헬퍼 ---

        public RcScreenMode GetScreenMode() => (RcScreenMode)ScreenMode;

        public void SetScreenMode(RcScreenMode mode)
        {
            ScreenMode = (int)mode;
            Save();
        }
    }
}
