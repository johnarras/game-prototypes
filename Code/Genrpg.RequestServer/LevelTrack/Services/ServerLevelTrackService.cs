using Genrpg.RequestServer.Core;
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
        Task<List<Reward>> GiveLevelTrackRewards(WebContext context, bool didJustLevelUp);
    }

    public class ServerLevelTrackService : IServerLevelTrackService
    {

        private IRewardService _rewardService = null;
        private IGameData _gameData = null;

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

                List<Reward> rewards = await GiveLevelTrackRewards(context, true);

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

        protected List<long> GetExcludedRepeatRewards()
        {
            return new List<long>() { EntityTypes.CoreCurrency, EntityTypes.TradeGood };
        }

        public async Task<List<Reward>> GiveLevelTrackRewards(WebContext context, bool didJustLevelup)
        {

            List<Reward> retval = new List<Reward>();
            CoreData coreData = await context.GetAsync<CoreData>();

            LevelTrackRewardSettings rewardSettings = _gameData.Get<LevelTrackRewardSettings>(coreData);

            List<LevelTrackReward> rewards = rewardSettings.GetData().Where(x => x.Level <= coreData.Level).OrderBy(x => x.Level).ToList();

            List<LevelTrackReward> permRewards = rewards.Where(x => !GetExcludedRepeatRewards().Contains(x.EntityTypeId)).ToList();

            List<LevelTrackReward> oneTimeRewards = rewards.Except(permRewards).ToList();

            if (didJustLevelup)
            {
                oneTimeRewards = oneTimeRewards.Where(x => x.Level == coreData.Level).ToList();
            }
            else
            {
                oneTimeRewards.Clear();
            }

            List<LevelTrackReward> finalRewards = permRewards.Concat(oneTimeRewards).ToList();

            foreach (LevelTrackReward rew in finalRewards)
            {
                await _rewardService.GiveReward(context, rew, null);
            }

            return retval;
        }
    }
}
