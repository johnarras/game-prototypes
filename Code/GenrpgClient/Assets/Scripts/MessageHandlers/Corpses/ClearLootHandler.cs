using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using UnityEngine;


namespace Assets.Scripts.MessageHandlers.Corpses
{
    public class ClearLootHandler : BaseClientMapMessageHandler<ClearLoot>
    {
        protected override async Awaitable InnerProcess(ClearLoot msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                unit.Loot = null;
            }
        }
    }
}


