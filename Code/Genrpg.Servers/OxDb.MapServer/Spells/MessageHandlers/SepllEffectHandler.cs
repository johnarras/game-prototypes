using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class SepllEffectHandler : BaseMapObjectServerMapMessageHandler<ActiveSpellEffect>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, ActiveSpellEffect message)
        {
            _spellService.ApplyOneEffect(rand.Rand, message);
            await Task.CompletedTask;
        }
    }
}


