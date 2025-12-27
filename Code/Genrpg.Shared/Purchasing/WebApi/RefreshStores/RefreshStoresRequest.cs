using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.RefreshStores
{
    public class RefreshStoresRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


