using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Buildings.MapObjects;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Factories;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Zones.WorldData;

namespace OxDb.SharedGame.Buildings.Factories
{
    public class BuildingObjectFactory : BaseMapObjectFactory
    {
        public override long HelperKey => EntityTypes.Building;

        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            Building obj = new Building();
            obj.CopyDataToMapObjectFromMapSpawn(spawn);
            Zone zone = _mapProvider.GetMap().Get<Zone>(obj.ZoneId);
            if (zone != null)
            {
                obj.Level = zone.Level;
            }

            return obj;
        }
    }
}


