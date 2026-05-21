using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class SpellHitHandler : BaseMapObjectServerMapMessageHandler<SpellHit>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, SpellHit message)
        {
            _spellService.OnSpellHit(rand.Rand, message);
        }
    }
}


