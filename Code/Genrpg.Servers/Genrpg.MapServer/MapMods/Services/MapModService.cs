using Genrpg.MapServer.MapMods.Helpers;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.MapMods.MapObjectAddons;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.MapMods.Services
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


