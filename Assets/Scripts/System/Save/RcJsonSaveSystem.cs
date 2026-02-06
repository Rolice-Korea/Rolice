using System;
using System.IO;
using Rolice.Data;
using UnityEngine;

namespace Rolice.System
{
    /// <summary>
    /// JSON 파일 기반 저장 시스템
    /// </summary>
    public class RcJsonSaveSystem : IRcSaveSystem
    {
        private const string FileName = "player_progress.json";
        private readonly string _savePath;

        public RcJsonSaveSystem()
        {
            _savePath = Path.Combine(Application.persistentDataPath, FileName);
        }

        public void Save(RcPlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_savePath, json);
                Debug.Log($"[SaveSystem] 저장 완료: {_savePath}");
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
                {
                    Debug.Log("[SaveSystem] 저장 데이터 없음, 새 데이터 생성");
                    return RcPlayerData.CreateNew();
                }

                string json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<RcPlayerData>(json);
                data.RebuildCache();

                Debug.Log($"[SaveSystem] 로드 완료: 스테이지 {data.StageProgressList.Count}개");
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
            return File.Exists(_savePath);
        }

        public void Delete()
        {
            try
            {
                if (!HasSaveData()) return;
                
                File.Delete(_savePath);
                Debug.Log("[SaveSystem] 저장 데이터 삭제 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 삭제 실패: {e.Message}");
            }
        }
    }
}
