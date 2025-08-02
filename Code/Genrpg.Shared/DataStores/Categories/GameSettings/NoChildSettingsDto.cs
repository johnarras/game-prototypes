using Genrpg.Shared.DataStores.Categories.PlayerData.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class NoChildSettingsDto<TSettings> : StubGameSettings, ITopLevelSettings where TSettings : NoChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public TSettings Parent { get; set; }

        public virtual ITopLevelSettings Unpack() { return Parent; }

        public virtual void SetupForEditor(List<object> saveObjects)
        {

        }

    }
}
