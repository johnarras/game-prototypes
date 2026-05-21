using OxDb.MapServer.MapMods.Helpers;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMods.MapObjectAddons;
using OxDb.SharedGame.MapMods.MapObjects;

namespace OxDb.MapServer.MapMods.Services
{
    public class MapModService : IMapModService
    {
        private SetupDictionaryContainer<long, IMapModEffectHelper> _effects = new();

        protected IMapModEffectHelper GetHelper(long mapModEffectTypeId)
        {
            if (_effects.TryGetValue(mapModEffectTypeId, out IMapModEffectHelper helper))
            {
                return helper;
            }
            return null;
        }

        public void Process(IRandom rand, MapMod mapMod)
        {
            MapModAddon addon = mapMod.GetAddon<MapModAddon>();
            if (addon == null)
            {
                return;
            }

            foreach (MapModEffect effect in addon.Effects)
            {
                IMapModEffectHelper helper = GetHelper(effect.MapModEffectTypeId);
                if (helper != null)
                {
                    helper.Process(rand, mapMod, addon, effect);
                }
            }
        }
    }
}


