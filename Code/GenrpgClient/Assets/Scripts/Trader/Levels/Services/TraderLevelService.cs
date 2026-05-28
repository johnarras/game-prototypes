using Assets.Scripts.Awaitables;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.Trader.Levels.UI;
using NUnit.Framework;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.LevelTracks.Settings;
using OxDb.SharedGame.LevelTracks.WebApi;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Scripts.Trader.Levels.Services
{
    public interface ITraderLevelService : IInitializable
    {
        Awaitable ShowLevelGain(GainExpResponse response, bool showDoobersHere, long rewardSourceId);
    }
    public class TraderLevelService : ITraderLevelService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IDispatcher _dispatcher = null;
        private TraderLevelUI _levelUI = null;
        private IRewardService _rewardService = null;
        private IAwaitableService _awaitableService = null;
        private IDynamicUIService _dynamicUIService = null;
        private IAnalyticsService _analyticsService = null;

        public async Task Initialize(CancellationToken token)
        {
            _dispatcher.AddListener<TraderLevelUI>(OnTraderLevelUI, token);
            await Task.CompletedTask;
        }

        private void OnTraderLevelUI(TraderLevelUI levelUI)
        {
            _levelUI = levelUI;
        }

        public async Awaitable ShowLevelGain(GainExpResponse response, bool showDoobersHere, long rewardSourceId)
        {
            if (response == null || _levelUI == null)
            {
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            LevelTrackDifficultySettings difficultySettings = _gameData.Get<LevelTrackDifficultySettings>(_gs.ch);

            if (rewardSourceId != RewardSources.TravelReward)
            {
                _analyticsService.TrackEconomyEvent(AnalyticsEventNames.RewardInflow, EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, response.ExpGained, rewardSourceId);
            }
            if (showDoobersHere)
            {
                _dynamicUIService.ShowDefaultEntityDoober(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, response.ExpGained);
            }

            if (response.LevelsGained.Count > 0)
            {
                await Awaitable.WaitForSecondsAsync(0.5f);
                foreach (LevelGained gained in response.LevelsGained)
                {
                    _analyticsService.TrackEvent(AnalyticsEventNames.GainLevel, null, new Dictionary<string, double>()
                    {
                        [AnalyticsKeys.Level] = gained.NewLevel
                    });

                    if (gained.Rewards.Count > 0)
                    {
                        foreach (Reward rew in gained.Rewards)
                        {
                            await _rewardService.GiveReward(_gs.ch, rew, RewardSources.LevelUp, new ClientRewardParams(false, false, true));
                        }
                        await Awaitable.WaitForSecondsAsync(0.5f);
                    }

                    coreData.Level = gained.NewLevel;
                    coreData.Currencies[CoreCurrencyTypes.Exp] = 0;
                    coreData.Vars[TraderVars.ExpToLevelUp] = difficultySettings.GetExpToNextLevel(coreData.Level);

                    _levelUI.ShowCurrentData();
                }
            }

            _awaitableService.ForgetAwaitable(ShowFinalProgress(coreData, response));

        }

        private async Awaitable ShowFinalProgress(CoreData coreData, GainExpResponse response)
        {
            await Awaitable.WaitForSecondsAsync(0.5f);
            coreData.Currencies[CoreCurrencyTypes.Exp] = response.EndExp;
            coreData.Vars[TraderVars.ExpToLevelUp] = (int)response.EndExpToLevel;
            coreData.Level = response.EndLevel;
            _levelUI.ExpBar.SetValue(response.EndExp);

        }
    }
}
