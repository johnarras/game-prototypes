using OxDb.SharedCore.Interfaces;


public class LocalUserData : IStringId
{
    public string Id { get; set; }
    public string AccountId { get; set; }
    public string UserId { get; set; }
    public string LoginToken { get; set; }
    public bool IsFullScreen { get; set; }
}

