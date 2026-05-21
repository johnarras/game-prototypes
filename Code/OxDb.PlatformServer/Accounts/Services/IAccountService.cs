using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.ServerCore.Platform.WebApi;
using OxDb.SharedCore.Interfaces;

namespace OxDb.PlatformServer.Accounts.Services
{
    public interface IAccountService : IInitializable
    {
        void AddAccountToProductGraph(Account account, long accountProductId, string referrerId, bool justNewProduct);

        Task<string> GetNewUserId();

        Task<ProductToPlatformAuthResponse> HandleGameAuthRequest(ProductToPlatformAuthRequest request);
    }
}


