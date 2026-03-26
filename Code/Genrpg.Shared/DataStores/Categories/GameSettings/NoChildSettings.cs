using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class NoChildSettings : TopLevelGameSettings
    {
        public override ITopLevelSettings Unpack() { return this; }
    }
}


