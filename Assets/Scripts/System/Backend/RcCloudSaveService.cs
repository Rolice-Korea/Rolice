using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Rolice.Data;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Rolice.System.Backend
{
    public sealed class RcCloudSaveService : ICloudSyncService
    {
        private const string PlayerDataKey = "player_data";

        public async UniTask SaveAsync(RcPlayerData data)
        {
            string json = JsonUtility.ToJson(data);
            var dict = new Dictionary<string, object> { { PlayerDataKey, json } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dict);
        }

        public async UniTask<RcPlayerData> LoadAsync()
        {
            var keys   = new HashSet<string> { PlayerDataKey };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (!result.TryGetValue(PlayerDataKey, out var item))
                return null;

            string json = item.Value.GetAs<string>();
            var data = JsonUtility.FromJson<RcPlayerData>(json);
            data?.RebuildCache();
            return data;
        }
    }
}
