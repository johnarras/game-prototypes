using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    
    // MessagePackIgnore
    public abstract class StubGameSettings : IGameSettings
    {
        [IgnoreMember] public abstract string Id { get; set; }

        public virtual void SetInternalIds() { }
        public virtual void AddTo(GameData gameData) { }
        public void ClearIndex() { }

        [IgnoreMember] public DateTime SaveTime { get; set; }

        public virtual List<IGameSettings> GetChildren() { return new List<IGameSettings>(); }

    }
}
