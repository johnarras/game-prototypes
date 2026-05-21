using OxDb.SharedCore.GameSettings.Interfaces;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{
    public abstract class NoChildSettings : TopLevelGameSettings
    {
        public override ITopLevelSettings Unpack() { return this; }
    }
}


