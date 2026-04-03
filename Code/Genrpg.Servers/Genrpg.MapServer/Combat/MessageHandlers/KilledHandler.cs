using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.Levelup.Services;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.RpgLevels.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.WorldData;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class KilledHandler : BaseUnitServerMapMessageHandler<Killed>
    {
        private IRpgLevelService _levelService = null;
        private IMapProvider _mapProvider = null;

        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, Killed message)
        {
            _aiService.EndCombat(rand, unit, message.UnitId, false);
            if (unit is Character ch)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(message.ZoneId);
                if (zone != null)
                {
                    RpgLevel level = _gameData.Get<RpgLevelSettings>(unit).Get(zone.Level);

                    if (level != null)
                    {
                        await _rewardService.GiveReward(ch, EntityTypes.CharCurrency, CharCurrencyTypes.Exp, level.MobExp, null, 0, null);
                        _levelService.UpdateLevel(rand, ch);
                    }
                }
            }
        }
    }
}


