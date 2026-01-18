using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{

    public interface IFilteredObject : IStringId
    {
        DateTime CreationDate { get; set; }
        string ClientVersion { get; set; }
        GameDataOverrideList DataOverrides { get; set; }
        long Level { get; set; }

        string GetId();
    }
}


