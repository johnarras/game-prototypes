using OxDb.SharedCore.Website.Requests.Interfaces;

namespace OxDb.ServerCore.Platform.WebApi
{
    public class ProductToPlatformAuthRequest : IWebRequest
    {
        public long ProductId { get; set; }
        public string AccountId { get; set; }
        public string ProductUserId { get; set; }
        public long DataBits { get; set; }
        public string SessionId { get; set; }
    }
}
