using OxDb.SharedCore.DataStores.Interfaces;
using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.Interfaces
{
    public interface IGameSettings : ISearchableItem
    {
        void SetInternalIds();
        void ClearIndex();
        List<IGameSettings> GetChildren();
        DateTime SaveTime { get; set; }
    }
}


