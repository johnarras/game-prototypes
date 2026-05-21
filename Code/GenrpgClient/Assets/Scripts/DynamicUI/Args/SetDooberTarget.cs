
using Assets.Scripts.DynamicUI.Interfaces;
using OxDb.SharedCore.Client.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Doobers.Events
{
    public class SetDooberTarget : IClientEvent
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public GameObject Target { get; set; }

        public bool IsMainDooberTarget { get; set; }


        public IEntityQuantityIcon EntityQuantityIcon { get; set; }

        public SetDooberTarget(long entityTypeId, long entityId, GameObject target, bool isMainDooberTarget, IEntityQuantityIcon targetIcon)
        {
            EntityTypeId = entityTypeId;
            EntityId = entityId;
            Target = target;
            IsMainDooberTarget = isMainDooberTarget;
            EntityQuantityIcon = targetIcon;
        }
    }
}


