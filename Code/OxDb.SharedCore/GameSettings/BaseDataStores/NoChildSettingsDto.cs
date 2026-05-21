using OxDb.SharedCore.GameSettings.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
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


