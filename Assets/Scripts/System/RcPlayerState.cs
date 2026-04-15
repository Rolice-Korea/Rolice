using System;
using Cysharp.Threading.Tasks;
using Engine;
using Rolice.Data;
using Rolice.System.Backend;
using Rolice.System.Economy;
using UnityEngine;

namespace Rolice.System
{
    public class RcPlayerState : RcSingleton<RcPlayerState>
    {
        private const string     CloudKey   = "player_data";

        private RcPlayerData     _data;
        private RcJsonSaveSystem _localSave;

        public bool IsInitialized { get; private set; }
        public bool IsSynced      { get; private set; }
        public event Action OnProgressChanged;
        public RcPlayerData Data => _data;

        public void Initialize()
        {
            _localSave    = new RcJsonSaveSystem();
            _data         = _localSave.Load();
            IsInitialized = true;
        }

        // 클라우드 → 로컬 동기화. 실패 시 예외.
        public async UniTask SyncFromCloudAsync()
        {
            await RcBackendServices.Auth.EnsureAuthAsync();

            string json = await RcBackendServices.CloudSync.LoadAsync(CloudKey);

            if (json != null)
            {
                _data = JsonUtility.FromJson<RcPlayerData>(json);
                _data?.RebuildCache();
            }
            else
            {
                // 최초 가입: 새 데이터로 초기화 후 서버에 업로드
                _data = RcPlayerData.CreateNew();
                await RcBackendServices.CloudSync.SaveAsync(CloudKey, JsonUtility.ToJson(_data));
            }

            // 재화/아이템은 별도 Firestore 경로에서 동기화
            if (RcBackendServices.Economy is RcFirestoreEconomyService firestoreEconomy)
                await firestoreEconomy.SyncFromCloudAsync();

            _localSave.Save(_data);
            IsSynced = true;
            NotifyChanged();
        }

        // 로컬 캐시에만 저장 (읽기 캐시 갱신용)
        public void SaveLocal() => _localSave.Save(_data);

        // 진행도 등 player_data를 클라우드에 저장. 실패 시 예외.
        public async UniTask SaveToCloudAsync()
        {
            await RcBackendServices.Auth.EnsureAuthAsync();
            await RcBackendServices.CloudSync.SaveAsync(CloudKey, JsonUtility.ToJson(_data));
        }

        public void NotifyChanged() => OnProgressChanged?.Invoke();

        public async UniTask ResetAllAsync()
        {
            _localSave.Delete();
            _data = RcPlayerData.CreateNew();
            await SaveToCloudAsync();
            NotifyChanged();
        }
    }
}
