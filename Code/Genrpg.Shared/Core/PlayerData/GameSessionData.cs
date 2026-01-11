using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Core.PlayerData
{
    public class GameSessionData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string SessionId { get; set; }
    }
}
