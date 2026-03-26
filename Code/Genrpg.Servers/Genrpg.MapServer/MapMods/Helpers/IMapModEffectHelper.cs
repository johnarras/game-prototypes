using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMods.MapObjectAddons;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.MapMods.Helpers
{
    public interface IMapModEffectHelper : ISetupDictionaryItem<long>
    {
        void Process(IRandom rand, MapMod mapMod, MapModAddon addon, MapModEffect effect);
    }
}


