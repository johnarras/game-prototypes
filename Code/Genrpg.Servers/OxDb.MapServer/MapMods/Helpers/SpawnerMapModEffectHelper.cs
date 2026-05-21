using OxDb.MapServer.Maps;
using OxDb.MapServer.Spawns.MapObjectAddons;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMods.Constants;
using OxDb.SharedGame.MapMods.MapObjectAddons;
using OxDb.SharedGame.MapMods.MapObjects;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Pathfinding.Services;
using OxDb.SharedGame.Spawns.WorldData;
using System;
using System.Collections.Generic;

namespace OxDb.MapServer.MapMods.Helpers
{
    public class SpawnerMapModEffectHelper : IMapModEffectHelper
    {
        private IMapObjectManager _objectManager = null;
        private IPathfindingService _pathfindingService = null;
        protected IFullRepositoryService _repoService = null;

        const int MinSeparation = 5;

        public long HelperKey => MapModEffects.Spawner;

        public void Process(IRandom rand, MapMod mapMod, MapModAddon addon, MapModEffect effect)
        {
            if (effect.CurrQuantity >= effect.MaxQuantity)
            {
                return;
            }

            List<MapObject> nearbyObjects = _objectManager.GetObjectsNear(mapMod.X, mapMod.Z, null, addon.Radius + 10);

            int childQuantity = 0;

            foreach (MapObject nearby in nearbyObjects)
            {
                DynamicSpawnAddon dynamicAddon = nearby.GetAddon<DynamicSpawnAddon>();

                if (dynamicAddon != null && dynamicAddon.ParentId == mapMod.Id)
                {
                    childQuantity++;
                }
            }

            if (childQuantity >= effect.MaxQuantity)
            {
                return;
            }

            for (int times = 0; times < 10; times++)
            {
                int xpos = (int)(mapMod.X + RandUtils.DeltaRange(addon.Radius, rand));
                int zpos = (int)(mapMod.Z + RandUtils.DeltaRange(addon.Radius, rand));

                if (_pathfindingService.CellIsBlocked(xpos, zpos))
                {
                    continue;
                }

                foreach (MapObject mo in nearbyObjects)
                {
                    if (Math.Abs(mo.X - xpos) <= MinSeparation &&
                        Math.Abs(mo.Z - zpos) <= MinSeparation)
                    {
                        continue;
                    }
                }

                DynamicSpawnAddon dynamicAddon = new DynamicSpawnAddon() { ParentId = mapMod.Id };

                List<IMapObjectAddon> newAddons = new List<IMapObjectAddon>() { dynamicAddon };

                MapSpawn newObjectSpawn = new MapSpawn()
                {
                    EntityTypeId = mapMod.EntityTypeId,
                    EntityId = mapMod.EntityId,
                    X = xpos,
                    Z = zpos,
                    FactionTypeId = mapMod.FactionTypeId,
                    LocationId = mapMod.LocationId,
                    LocationPlaceId = mapMod.LocationPlaceId,
                    ZoneId = mapMod.ZoneId,
                    Id = mapMod.Id + "-" + (++addon.TriggerTimes),
                    Addons = newAddons,
                };

                _objectManager.SpawnObject(rand, newObjectSpawn);

                if (mapMod.Spawn is MapSpawn mapModSpawn)
                {
                    _repoService.QueueSave(mapModSpawn);
                }
                effect.CurrQuantity++;

                break;
            }

            return;
        }
    }
}


