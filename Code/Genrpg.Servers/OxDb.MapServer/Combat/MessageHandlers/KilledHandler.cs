using OxDb.MapServer.Combat.Messages;
using OxDb.MapServer.Levelup.Services;
using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.RpgLevels.Settings;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Zones.WorldData;
using System.Threading.Tasks;

namespace OxDb.MapServer.Combat.MessageHandlers
{
    public class KilledHandler : BaseUnitServerMapMessageHandler<Killed>
    {
        private IRpgLevelService _levelService = null;
        private IMapProvider _mapProvider = null;

        protected override async ValueTask InnerProcess(Unit unit, Killed message)
        {
            _aiService.EndCombat(unit, message.UnitId, false);
            if (unit is Character ch)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(message.ZoneId);
                if (zone != null)
                {
                    RpgLevel level = _gameData.Get<RpgLevelSettings>(unit).Get(zone.Level);

                    if (level != null)
                    {
                        await _rewardService.GiveReward(ch, EntityTypes.CharCurrency, CharCurrencyTypes.Exp, level.MobExp, RewardSources.Kill, null, 0, null);
                        await _levelService.UpdateLevel(ch);
                    }
                }
            }
        }
    }
}


