using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public interface ICloudSyncService
    {
        UniTask SaveAsync(string key, string json);
        UniTask<string> LoadAsync(string key);
    }
}
