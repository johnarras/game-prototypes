
using Assets.Scripts.Lockstep.Factions.Components;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Factions.Setup
{
    public class FactionInjector
    {
        public void InjectData(in EntityManager em, List<FactionConfig> factions)
        {
            foreach (FactionConfig config in factions)
            {
                Entity entity = em.CreateEntity();
                em.AddComponentData(entity, new FactionData
                {
                    FactionId = config.FactionId,
                    SpawnInterval = config.SpawnInterval,
                    SpawnChance = config.SpawnPercent,
                    UnitSpeed = config.UnitSpeed
                });
            }
        }
    }
}
