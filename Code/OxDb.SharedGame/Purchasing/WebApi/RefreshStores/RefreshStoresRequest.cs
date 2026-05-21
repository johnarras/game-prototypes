using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.Purchasing.WebApi.RefreshStores
{
    public class RefreshStoresRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


