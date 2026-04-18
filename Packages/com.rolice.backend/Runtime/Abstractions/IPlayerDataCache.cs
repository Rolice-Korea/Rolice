namespace Rolice.System.Backend
{
    public interface IPlayerDataCache
    {
        int  GetCurrency(string key);
        void SetCurrency(string key, int value);
        bool HasOwnedItem(string itemId);
        void AddOwnedItem(string itemId);
    }
}
