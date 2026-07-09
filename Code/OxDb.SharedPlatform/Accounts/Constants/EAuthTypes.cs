using System;

namespace OxDb.SharedPlatform.Accounts.Constants
{
    [Flags]
    public enum EAuthTypes
    {
        None = 0,
        Guest = 1 << 0,
        Email = 1 << 1,
        GooglePlay = 1 << 2,
        iOS = 1 << 3,
        Facebook = 1 << 4,
        Device = 1 << 5,
        Local = 1 << 6,
    }
}
