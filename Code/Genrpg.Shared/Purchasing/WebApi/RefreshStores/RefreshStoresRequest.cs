using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.RefreshStores
{
    public class RefreshStoresRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


