using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMods.MapObjectAddons;
using OxDb.SharedGame.MapMods.MapObjects;

namespace OxDb.MapServer.MapMods.Helpers
{
    public interface IMapModEffectHelper : ISetupDictionaryItem<long>
    {
        void Process(IRandom rand, MapMod mapMod, MapModAddon addon, MapModEffect effect);
    }
}


