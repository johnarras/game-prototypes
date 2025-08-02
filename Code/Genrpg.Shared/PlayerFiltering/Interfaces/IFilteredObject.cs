using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{
    public interface IFilteredObject : IStringId
    {
        int Level { get; set; }
        DateTime CreationDate { get; set; }
        GameDataOverrideList DataOverrides { get; set; }
        string ClientVersion { get; set; }     
    }
}
