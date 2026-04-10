using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudSave;

namespace Rolice.System.Backend
{
    public sealed class RcCloudSaveService : ICloudSyncService
    {
        public async UniTask SaveAsync(string key, string json)
        {
            var dict = new Dictionary<string, object> { { key, json } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dict);
        }

        public async UniTask<string> LoadAsync(string key)
        {
            var keys   = new HashSet<string> { key };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (!result.TryGetValue(key, out var item))
                return null;

            return item.Value.GetAs<string>();
        }
    }
}
