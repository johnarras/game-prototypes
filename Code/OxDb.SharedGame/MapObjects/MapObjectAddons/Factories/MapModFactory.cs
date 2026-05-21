using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMods.MapObjects;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Factories;
using OxDb.SharedGame.Spawns.Interfaces;

namespace OxDb.SharedGame.MapObjects.MapObjectAddons.Factories
{
    public class MapModFactory : BaseMapObjectFactory
    {
        public override long HelperKey => EntityTypes.MapMod;

        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            MapMod mapMod = new MapMod();
            mapMod.CopyDataToMapObjectFromMapSpawn(spawn);
            return mapMod;
        }
    }
}


