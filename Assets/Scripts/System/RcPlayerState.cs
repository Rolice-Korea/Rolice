using System;
using Cysharp.Threading.Tasks;
using Engine;
using Rolice.Data;
using Rolice.System.Backend;
using UnityEngine;

namespace Rolice.System
{
    public class RcPlayerState : RcSingleton<RcPlayerState>
    {
        private const string     CloudKey   = "player_data";

        private RcPlayerData     _data;
        private RcJsonSaveSystem _localSave;

        public bool IsInitialized { get; private set; }
        public event Action OnProgressChanged;
        public RcPlayerData Data => _data;

        public void Initialize()
        {
            _localSave    = new RcJsonSaveSystem();
            _data         = _localSave.Load();
            IsInitialized = true;
        }

        // 클라우드 → 로컬 동기화. 성공 여부 반환.
        public async UniTask<bool> SyncFromCloudAsync()
        {
            try
            {
                await RcBackendServices.Auth.EnsureAuthAsync();
                string json = await RcBackendServices.CloudSync.LoadAsync(CloudKey);

                if (json != null)
                {
                    _data = JsonUtility.FromJson<RcPlayerData>(json);
                    _data?.RebuildCache();
                    _localSave.Save(_data);
                    NotifyChanged();
                }
                else
                {
                    await RcBackendServices.CloudSync.SaveAsync(CloudKey, JsonUtility.ToJson(_data));
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerState] 클라우드 동기화 실패: {e.Message}");
                return false;
            }
        }

        // 로컬에만 저장
        public void SaveLocal() => _localSave.Save(_data);

        // 클라우드에 저장. 성공 여부 반환.
        public async UniTask<bool> SaveToCloudAsync()
        {
            if (!RcBackendServices.Auth.IsAuthenticated)
            {
                try { await RcBackendServices.Auth.EnsureAuthAsync(); }
                catch { return false; }
            }

            try
            {
                await RcBackendServices.CloudSync.SaveAsync(CloudKey, JsonUtility.ToJson(_data));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerState] 클라우드 저장 실패: {e.Message}");
                return false;
            }
        }

        public void NotifyChanged() => OnProgressChanged?.Invoke();

        public void ResetAll()
        {
            _localSave.Delete();
            _data = RcPlayerData.CreateNew();
            SaveToCloudAsync().Forget();
            NotifyChanged();
        }
    }
}
