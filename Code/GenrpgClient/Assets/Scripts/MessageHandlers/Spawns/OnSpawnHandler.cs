
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using UnityEngine;

public class OnSpawnHandler : BaseClientMapMessageHandler<OnSpawn>
{
    protected override async Awaitable InnerProcess(OnSpawn spawnMessage, CancellationToken token)
    {
        if (_objectManager.GetMapObject(spawnMessage.ObjId, out MapObject obj))
        {
            if (obj is Unit existingUnit)
            {
                existingUnit.AddFlag(spawnMessage.TempFlags);

                existingUnit.Stats.UpdateFromSnapshot(spawnMessage.Stats);

                existingUnit.Loot = spawnMessage.Loot;
                existingUnit.SkillLoot = spawnMessage.SkillLoot;

                if (existingUnit.HasFlag(UnitFlags.IsDead))
                {
                    if (_objectManager.GetController(spawnMessage.ObjId, out UnitController controller))
                    {
                        Died died = new Died()
                        {
                            UnitId = spawnMessage.ObjId,
                        };
                        controller.OnDeath(died, token);
                    }
                }
            }
            return;
        }

        MapObject newObj = _objectManager.SpawnObject(spawnMessage);

        if (newObj != null)
        {
            _objectManager.AddObject(newObj, null);

            IMapObjectLoader loader = _objectManager.GetMapObjectLoader(spawnMessage.EntityTypeId);

            if (loader != null)
            {
                await loader.Load(spawnMessage, newObj, token);
            }
        }
    }
}


