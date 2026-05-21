using OxDb.PlatformServer.Accounts.Services;
using OxDb.ServerCore.Platform.WebApi;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.Platform.Services
{

    public interface IGameToPlatformAuthService : IInjectable
    {
        Task<ProductToPlatformAuthResponse> CheckPlatformAuth(ProductToPlatformAuthRequest request);
    }
    public class GameToPlatformAuthService : IGameToPlatformAuthService
    {
        // Maybe do this someday.
        // private IWebRequestService _webRequestService = null;


        private IAccountService _accountService = null;
        public async Task<ProductToPlatformAuthResponse> CheckPlatformAuth(ProductToPlatformAuthRequest request)
        {

            // Maybe do this someday.
            // return await _webRequestService.PostAsync<ProductToPlatformAuthRequest,ProductToPlatformAuthResponse>("platform-auth-endpoint", request);
            return await _accountService.HandleGameAuthRequest(request);
        }
    }
}
