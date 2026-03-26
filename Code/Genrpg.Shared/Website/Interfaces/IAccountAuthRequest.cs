namespace Genrpg.Shared.Website.Interfaces
{
    public interface IAccountAuthRequest : IWebRequest
    {
        long ProductId { get; set; }
        string ReferrerId { get; set; }
        string DeviceId { get; set; }
        string Password { get; set; }
    }
}


