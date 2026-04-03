using Genrpg.RequestServer.Core;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.LevelTracks.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Constants;

namespace Genrpg.RequestServer.LevelTrack.Services
{

    public interface IServerLevelTrackService : IInjectable
    {
        Task<GainExpResponse> GainExp(WebContext context, long newExp, bool sendResponseToClient);
    }

    public class ServerLevelTrackService : IServerLevelTrackService
    {

        private IRewardService _rewardService = null;
        private IGameData _gameData = null;
        private ICalcAttributeService _calcAttributeService = null;

        public async Task<GainExpResponse> GainExp(WebContext context, long expGained, bool sendResponseToClient)
        {

            if (expGained == 0)
            {
                return null;
            }

            CoreData coreData = await context.GetAsync<CoreData>();

            GainExpResponse response = new GainExpResponse();

            LevelTrackDifficultySettings diffSettings = _gameData.Get<LevelTrackDifficultySettings>(coreData);

            response.StartExp = coreData.Currencies[CoreCurrencyTypes.Exp];
            response.StartLevel = coreData.Level;
            response.StartExpToLevelUp = coreData.Vars[TraderVars.ExpToLevelUp];
            response.ExpGained = expGained;
            coreData.Currencies.Add(CoreCurrencyTypes.Exp, (int)expGained);

            // Just gain Exp, no new rewards.
            if (coreData.Currencies[CoreCurrencyTypes.Exp] < coreData.Vars[TraderVars.ExpToLevelUp])
            {

                response.EndExp = coreData.Currencies[CoreCurrencyTypes.Exp];
                response.EndLevel = coreData.Level;
                response.EndExpToLevel = coreData.Vars[TraderVars.ExpToLevelUp];

                if (sendResponseToClient)
                {
                    context.AddResponse(response);
                }

                return response;
            }

            // Gained at least one level.

            while (coreData.Currencies[CoreCurrencyTypes.Exp] > coreData.Vars[TraderVars.ExpToLevelUp])
            {
                coreData.Currencies.Add(CoreCurrencyTypes.Exp, -coreData.Vars[TraderVars.ExpToLevelUp]);

                coreData.Level++;
                coreData.Vars[TraderVars.ExpToLevelUp] = (int)diffSettings.GetExpToNextLevel(coreData.Level);

                List<Reward> rewards = await _calcAttributeService.CalcBaseAttributes(context, true);

                response.LevelsGained.Add(new LevelGained()
                {
                    NewLevel = coreData.Level,
                    Rewards = rewards,
                });

            }

            response.EndExp = coreData.Currencies[CoreCurrencyTypes.Exp];
            response.EndLevel = coreData.Level;
            response.EndExpToLevel = coreData.Vars[TraderVars.ExpToLevelUp];

            return response;
        }

    }
}
