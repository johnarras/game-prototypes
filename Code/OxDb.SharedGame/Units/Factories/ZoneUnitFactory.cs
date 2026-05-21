using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.AI.Settings;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Factories;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Spells.Settings.Spells;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Factories
{
    public class ZoneUnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService = null;
        public override long HelperKey => EntityTypes.ZoneUnit;
        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {

            Map map = _mapProvider.GetMap();

            Zone spawnZone = _mapProvider.GetMap().Get<Zone>(spawn.EntityId);

            if (spawnZone == null)
            {
                spawnZone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);
            }

            if (spawnZone == null)
            {
                return null;
            }

            if (_mapProvider.GetMap().OverrideZoneId > 0 && _mapProvider.GetMap().OverrideZonePercent >= spawn.OverrideZonePercent)
            {
                Zone newSpawnZone = _mapProvider.GetMap().Get<Zone>(_mapProvider.GetMap().OverrideZoneId);
                if (newSpawnZone != null)
                {
                    spawnZone = newSpawnZone;
                }
            }

            Zone levelZone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);

            if (levelZone == null)
            {
                return null;
            }

            UnitType utype = _unitGenService.GetRandomUnitType(rand, _mapProvider.GetMap(), spawnZone);

            if (utype == null)
            {
                return null;
            }

            long level = levelZone.GetFinalUnitLevel(rand, spawn.X, spawn.Z, levelZone.Level, _mapProvider.GetMap().MaxLevel);


            Unit unit = new Unit();
            unit.Level = level;
            unit.CopyDataToMapObjectFromMapSpawn(spawn);
            unit.EntityTypeId = EntityTypes.Unit;
            unit.EntityId = utype.IdKey;
            unit.BaseSpeed = _gameData.Get<AISettings>(unit).BaseUnitSpeed;
            unit.Speed = unit.BaseSpeed;

            if (spawn is OnSpawn onSpawn)
            {
                unit.AddFlag(onSpawn.TempFlags);
            }

            SpellType spellType = _gameData.Get<SpellTypeSettings>(unit).Get(1);

            Spell spell = _serializer.ConvertType<SpellType, Spell>(spellType);

            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(unit).GetData();

            spell.ElementTypeId = etypes[rand.Next() % etypes.Count].IdKey;
            spell.Id = HashUtils.NewGuid();

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = _unitGenService.GenerateUnitName(rand, utype.IdKey, spawnZone.IdKey, null);

            _statService.CalcStats(unit, true);

            return unit;
        }
    }
}


