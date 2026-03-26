using Assets.Scripts.Awaitables;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.Trader.Levels.UI;
using Assets.Scripts.Core;
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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Levels.Services
{
    public interface ITraderLevelService : IInitializable
    {
        Awaitable ShowLevelGain(GainExpResponse response, bool showDoobersHere);
    }
    public class TraderLevelService : ITraderLevelService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IDispatcher _dispatcher = null;
        private TraderLevelUI _levelUI = null;
        private IRewardService _rewardService = null;
        private IClientRandom _random = null;
        private IAwaitableService _awaitableService = null;
        private IDynamicUIService _dynamicUIService = null;

        public async Task Initialize(CancellationToken token)
        {
            _dispatcher.AddListener<TraderLevelUI>(OnTraderLevelUI, token);
            await Task.CompletedTask;
        }

        private void OnTraderLevelUI(TraderLevelUI levelUI)
        {
            _levelUI = levelUI;
        }

        public async Awaitable ShowLevelGain(GainExpResponse response, bool showDoobersHere)
        {
            if (response == null || _levelUI == null)
            {
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            LevelTrackDifficultySettings difficultySettings = _gameData.Get<LevelTrackDifficultySettings>(_gs.ch);

            if (showDoobersHere)
            {
                _dynamicUIService.ShowDefaultEntityDoober(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, response.ExpGained);
            }

            if (response.LevelsGained.Count > 0)
            {
                await Awaitable.WaitForSecondsAsync(0.5f);
                foreach (LevelGained gained in response.LevelsGained)
                {
                    await _levelUI.AnimateToEndOfBar();

                    if (gained.Rewards.Count > 0)
                    {
                        foreach (Reward rew in gained.Rewards)
                        {
                            await _rewardService.GiveReward(_gs.ch, rew, new ClientRewardParams(false, false));
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
