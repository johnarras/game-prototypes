using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Purchasing.PlayerData;

namespace OxDb.SharedGame.Purchasing.WebApi.RefreshStores
{
    public class RefreshStoresResponse : IWebResponse
    {
        public PlayerStoreOfferData Stores { get; set; }
    }
}


