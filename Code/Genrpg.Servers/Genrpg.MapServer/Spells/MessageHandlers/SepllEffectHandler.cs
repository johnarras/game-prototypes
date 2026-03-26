using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class SepllEffectHandler : BaseMapObjectServerMapMessageHandler<ActiveSpellEffect>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, ActiveSpellEffect message)
        {
            _spellService.ApplyOneEffect(rand, message);
            await Task.CompletedTask;
        }
    }
}


