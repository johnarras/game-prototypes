using OxDb.SharedCore.GameSettings.PlayerData;
using OxDb.SharedCore.Interfaces;
using System;

namespace OxDb.SharedCore.PlayerFiltering.Interfaces
{

    public interface IFilteredObject : IStringId
    {
        DateTime Created { get; set; }
        string Client { get; set; }
        ABList AB { get; set; }
        long Level { get; set; }

        string GetId();
    }
}


