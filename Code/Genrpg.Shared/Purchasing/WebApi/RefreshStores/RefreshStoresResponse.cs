using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.RefreshStores
{
    public class RefreshStoresResponse : IWebResponse
    {
        public PlayerStoreOfferData Stores { get; set; }
    }
}


