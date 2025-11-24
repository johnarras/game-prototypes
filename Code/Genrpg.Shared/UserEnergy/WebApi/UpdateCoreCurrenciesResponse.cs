using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.UserEnergy.WebApi
{
    [MessagePackObject]
    public class UpdateCoreCurrenciesResponse : IWebResponse
    {
        [Key(0)] public List<CoreCurrencyStatus> ChangedStatuses { get; set; } = new List<CoreCurrencyStatus>();
    }
}
