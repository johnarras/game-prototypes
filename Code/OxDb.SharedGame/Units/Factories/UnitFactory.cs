using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.AI.Settings;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Factories;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.Spawns.Interfaces;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Spells.Settings.Spells;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Units.Factories
{
    public class UnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService = null;
        public override long HelperKey => EntityTypes.Unit;
        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            UnitType utype = _gameData.Get<UnitTypeSettings>(null).Get(spawn.EntityId);

            if (utype != null && utype.IdKey == 0)
            {
                utype = null;
            }

            Zone zone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);

            long level = zone != null ? zone.Level : 1;
            if (utype == null)
            {
                if (spawn.ZoneId > 0)
                {
                    if (zone == null)
                    {
                        return null;
                    }
                    utype = _unitGenService.GetRandomUnitType(rand, _mapProvider.GetMap(), zone);
                }
            }

            if (zone != null)
            {
                level = zone.GetFinalUnitLevel(rand, spawn.X, spawn.Z, zone.Level, _mapProvider.GetMap().MaxLevel);
            }

            if (utype == null)
            {
                return null;
            }

            Unit unit = new Unit();
            unit.Level = level;

            if (spawn.GetAddons().Any())
            {
                unit.Level += 3;
            }

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

            foreach (SpellEffect effect in spell.Effects)
            {
                effect.Scale /= 3;
            }

            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(unit).GetData();

            spell.ElementTypeId = etypes[(rand.Next() % (etypes.Count - 1) + 1)].IdKey;
            spell.Id = HashUtils.NewGuid();
            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = spawn.Name;
            if (string.IsNullOrEmpty(unit.Name))
            {
                if (zone != null)
                {
                    unit.Name = _unitGenService.GenerateUnitName(rand, utype.IdKey, zone.IdKey, null);
                }
                else
                {
                    unit.Name = utype.Name;
                }
            }

            _statService.CalcStats(unit, true);

            return unit;
        }
    }
}


