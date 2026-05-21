using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.GroundObjects.MapObjects;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Factories;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Zones.WorldData;

namespace OxDb.SharedGame.GroundObjects.Factories
{
    public class GroundObjectFactory : BaseMapObjectFactory
    {
        public override long HelperKey => EntityTypes.GroundObject;

        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            GroundObject obj = new GroundObject();
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


