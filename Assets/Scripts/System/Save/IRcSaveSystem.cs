using Rolice.Data;

namespace Rolice.System
{
    public interface IRcSaveSystem
    {
        void Save(RcPlayerData data);
        RcPlayerData Load();
        bool HasSaveData();
        void Delete();
    }
}
