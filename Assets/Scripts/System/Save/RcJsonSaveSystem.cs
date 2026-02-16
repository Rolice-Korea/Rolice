using System;
using System.IO;
using Rolice.Data;
using UnityEngine;

namespace Rolice.System
{
    public class RcJsonSaveSystem : IRcSaveSystem
    {
        private const string FileName = "player_progress.json";
        private readonly string savePath;

        public RcJsonSaveSystem()
        {
            savePath = Path.Combine(Application.persistentDataPath, FileName);
        }

        public void Save(RcPlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(savePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 저장 실패: {e.Message}");
            }
        }

        public RcPlayerData Load()
        {
            try
            {
                if (!HasSaveData())
                    return RcPlayerData.CreateNew();

                string json = File.ReadAllText(savePath);
                var data = JsonUtility.FromJson<RcPlayerData>(json);
                data.RebuildCache();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 로드 실패: {e.Message}");
                return RcPlayerData.CreateNew();
            }
        }

        public bool HasSaveData()
        {
            return File.Exists(savePath);
        }

        public void Delete()
        {
            try
            {
                if (!HasSaveData()) return;
                File.Delete(savePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 삭제 실패: {e.Message}");
            }
        }
    }
}
