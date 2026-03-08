using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTrackDifficulty.Settings;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.LevelTracks.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using ZstdSharp.Unsafe;

namespace Genrpg.RequestServer.LevelTrack.Services
{

    public interface IServerLevelTrackService : IInjectable
    {
        Task<GainExpResponse> GainExp(WebContext context, long newExp, bool sendResponseToClient);
    }

    public class ServerLevelTrackService : IServerLevelTrackService
    {

        private IWebRewardService _rewardService = null;
        private IGameData _gameData = null;
        public async Task<GainExpResponse> GainExp(WebContext context, long expGained, bool sendResponseToClient)
        {

            if (expGained == 0)
            {
                return null;
            }

            CoreData coreData = context.core;

            GainExpResponse response = new GainExpResponse();

            IReadOnlyList<LevelTrackReward> rewardTrackList = _gameData.Get<LevelTrackRewardSettings>(coreData).GetData();

            LevelTrackReward currLevel = rewardTrackList.FirstOrDefault(x => x.Level == coreData.Level);

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

                LevelTrackReward reward = rewardTrackList.FirstOrDefault(x=>x.Level == coreData.Level);

                List<Reward> rewards = new List<Reward>();

                if (reward != null)
                {
                    rewards.Add(new Reward(reward));
                }
                else
                {
                    rewards.Add(new Reward() { EntityTypeId = EntityTypes.CoreCurrency, EntityId = CoreCurrencyTypes.Rations, Quantity = 50 });
                }

                RewardParams rp = new RewardParams();

                await _rewardService.GiveRewardsAsync(context, rewards, rp);

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
