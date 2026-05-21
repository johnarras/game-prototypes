using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spawns.WorldData;
using OxDb.SharedGame.Stats.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Factories
{
    public class ProxyCharacterFactory : UnitFactory
    {
        public override long HelperKey => EntityTypes.ProxyCharacter;

        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {

            MapSpawn unitSpawn = new MapSpawn()
            {
                ObjId = spawn.ObjId,
                EntityTypeId = EntityTypes.Unit,
                EntityId = spawn.EntityId,
                X = spawn.X,
                Z = spawn.Z,
            };

            Unit unit = base.Create(rand, unitSpawn) as Unit;

            if (unit == null)
            {
                return null;
            }
            unit.FactionTypeId = FactionTypes.Player;

            if (spawn is OnSpawn onSpawn)
            {
                List<FullStat> smallStats = onSpawn.Stats;

                unit.Stats.UpdateFromSnapshot(smallStats);

                unit.Level = onSpawn.Level;
                unit.Name = onSpawn.Name;
                unit.Speed = onSpawn.Speed;
                unit.BaseSpeed = onSpawn.Speed;
                unit.AddFlag(UnitFlags.ProxyCharacter);
            }
            return unit;
        }
    }
}


