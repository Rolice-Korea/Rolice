using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Rolice.System.Backend
{
    public sealed class RcUgsAuthService : IAuthService
    {
        public bool IsAuthenticated =>
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        public async UniTask EnsureAuthAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
