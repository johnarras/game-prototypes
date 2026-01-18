using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;

namespace Genrpg.Shared.Core.PlayerData
{
    [MessagePackObject]
    public class GameAccount : UniquePersonalUserData, IUserData
    {
        /// <summary>
        /// Used for the id found in the relational database
        /// </summary>
        /// 
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        [Key(2)] public string CurrCharId { get; set; }
        [Key(3)] public string ClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(4)] public string AccountId { get; set; }
        [Key(5)] public bool Deleted { get; set; }
        [Key(6)] public string ClientPlatformName { get; set; }
        [Key(7)] public string RefreshToken { get; set; }
        [Key(8)] public string GameUserId { get; set; }
        [Key(9)] public string SessionToken { get; set; }

    }
}


