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

            SyncFromCloudAsync().Forget(); // 앱 시작 시 백그라운드 동기화
        }

        public async UniTask SyncFromCloudAsync()
        {
            try
            {
                await RcBackendServices.Auth.EnsureAuthAsync();
                var cloudData = await RcBackendServices.CloudSync.LoadAsync();

                if (cloudData != null)
                {
                    _data = cloudData;
                    _localSave.Save(_data);
                    NotifyChanged();
                }
                else
                {
                    await RcBackendServices.CloudSync.SaveAsync(_data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerState] 클라우드 동기화 실패 (로컬 캐시 사용 중): {e.Message}");
            }
        }

        public void Save()
        {
            _localSave.Save(_data);
            SyncToCloudAsync().Forget();
        }

        public void SaveLocal() => _localSave.Save(_data);

        private async UniTaskVoid SyncToCloudAsync()
        {
            if (!RcBackendServices.Auth.IsAuthenticated)
            {
                try { await RcBackendServices.Auth.EnsureAuthAsync(); }
                catch { return; }
            }

            try
            {
                await RcBackendServices.CloudSync.SaveAsync(_data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerState] 클라우드 저장 실패: {e.Message}");
            }
        }

        public void NotifyChanged() => OnProgressChanged?.Invoke();

        public void ResetAll()
        {
            _localSave.Delete();
            _data = RcPlayerData.CreateNew();
            SyncToCloudAsync().Forget();
            NotifyChanged();
        }
    }
}
