using Genrpg.Shared.DataStores.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : ISearchableItem
    {
        void SetInternalIds();
        void ClearIndex();
        List<IGameSettings> GetChildren();
        DateTime SaveTime { get; set; }
    }
}


