using OxDb.SharedCore.Interfaces;
using OxDb.SharedPlatform.Accounts.Constants;


public class LocalUserData : IStringId
{
    public string Id { get; set; }
    public EAuthTypes LastAuthType { get; set; }
    public EAuthTypes ValidAuthTypes { get; set; }
    public string AccountId { get; set; }
    public string UserId { get; set; }
    public string LoginToken { get; set; }
    public string GuestAccountId { get; set; }
    public string GuestSecret { get; set; }


    public void ClearData()
    {
        LastAuthType = EAuthTypes.None;
        ValidAuthTypes = EAuthTypes.None;
        AccountId = null;
        UserId = null;
        LoginToken = null;
    }
}

