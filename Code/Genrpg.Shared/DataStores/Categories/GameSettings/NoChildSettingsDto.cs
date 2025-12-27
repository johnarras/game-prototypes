using Genrpg.Shared.GameSettings.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class NoChildSettingsDto<TSettings> : StubGameSettings, ITopLevelSettings where TSettings : NoChildSettings
    {
        [MessagePack.IgnoreMember] public abstract TSettings Parent { get; set; }

        public virtual ITopLevelSettings Unpack() { return Parent; }

        public virtual void SetupForEditor(List<object> saveObjects)
        {

        }

    }
}


