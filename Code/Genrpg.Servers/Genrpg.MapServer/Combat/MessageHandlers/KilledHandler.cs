using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.Levelup.Services;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.WorldData;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class KilledHandler : BaseUnitServerMapMessageHandler<Killed>
    {
        private IRpgLevelService _levelService = null;
        private IMapProvider _mapProvider = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, Killed message)
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
                        _rewardService.GiveReward(rand, ch, EntityTypes.Currency, CurrencyTypes.Exp, level.MobExp, null, null);
                        _levelService.UpdateLevel(rand, ch);
                    }
                }
            }
        }
    }
}
