using OxDb.SharedGame.SpellCrafting.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.SpellCrafting
{
    public class OnDeleteSpellHandler : BaseClientMapMessageHandler<OnDeleteSpell>
    {
        protected override async ValueTask InnerProcess(OnDeleteSpell msg, CancellationToken token)
        {
            _gs.ch.Get<SpellData>().Remove(msg.SpellId);
            _dispatcher.Dispatch(msg);
            await Task.CompletedTask;
        }
    }
}


