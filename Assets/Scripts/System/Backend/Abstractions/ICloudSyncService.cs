using Cysharp.Threading.Tasks;
using Rolice.Data;

namespace Rolice.System.Backend
{
    public interface ICloudSyncService
    {
        UniTask SaveAsync(RcPlayerData data);
        UniTask<RcPlayerData> LoadAsync();
    }
}
