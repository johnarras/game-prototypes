using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    [MessagePackObject]
    public class InitiatePurchaseResponse : IWebResponse
    {
        [Key(0)] public EInitiatePurchaseStates State { get; set; }
        [Key(1)] public string OfferId { get; set; }
        [Key(2)] public string UniqueId { get; set; }
        [Key(3)] public string BundleId { get; set; }
        [Key(4)] public string ProductId { get; set; }
    }
}
