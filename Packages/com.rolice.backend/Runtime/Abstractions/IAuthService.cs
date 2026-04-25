using Cysharp.Threading.Tasks;

namespace Rolice.System.Backend
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        UniTask EnsureAuthAsync();
        UniTask SignInAsync();
    }
}
