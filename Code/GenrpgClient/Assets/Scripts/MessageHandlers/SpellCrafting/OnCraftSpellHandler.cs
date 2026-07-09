using OxDb.SharedGame.SpellCrafting.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnCraftSpellHandler : BaseClientMapMessageHandler<OnCraftSpell>
    {
        protected override async ValueTask InnerProcess(OnCraftSpell msg, CancellationToken token)
        {

            _gs.ch.Get<SpellData>().Remove(msg.CraftedSpell.IdKey);
            _gs.ch.Get<SpellData>().Add(msg.CraftedSpell);
            _dispatcher.Dispatch(msg);
            await Task.CompletedTask;
        }
    }
}


