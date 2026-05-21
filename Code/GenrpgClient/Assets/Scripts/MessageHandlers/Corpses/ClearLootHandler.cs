using OxDb.SharedGame.Loot.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;
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
            await Task.CompletedTask;
        }
    }
}


