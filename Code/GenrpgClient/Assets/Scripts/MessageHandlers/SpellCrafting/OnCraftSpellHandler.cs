using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnCraftSpellHandler : BaseClientMapMessageHandler<OnCraftSpell>
    {
        protected override async Awaitable InnerProcess(OnCraftSpell msg, CancellationToken token)
        {

            _gs.ch.Get<SpellData>().Remove(msg.CraftedSpell.IdKey);
            _gs.ch.Get<SpellData>().Add(msg.CraftedSpell);
            _dispatcher.Dispatch(msg);
        }
    }
}


