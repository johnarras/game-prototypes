using OxDb.RequestServer.Core;
using OxDb.RequestServer.Trader.Travel.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Attributes.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.LevelTracks.Settings;
using OxDb.SharedGame.PlayMultiplier.Constants;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Holdings.PlayerData;

namespace OxDb.RequestServer.Trader.NewPlayer
{

    public interface ITraderNewPlayerService : IInjectable
    {
        Task UpdatePlayerOnLogin(WebContext context, bool onLogin);
    }

    public class TraderNewPlayerService : ITraderNewPlayerService
    {

        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;
        private IServerCaravanService _serverCaravanService = null;
        private ICalcAttributeService _calcAttributeService = null;

        public async Task UpdatePlayerOnLogin(WebContext context, bool onLogin)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            List<Reward> newRewards = new List<Reward>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(coreData);

            GameplayStatSettings statSettings = _gameData.Get<GameplayStatSettings>(coreData);

            AttributesData attributeData = await context.GetAsync<AttributesData>();

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            CaravanData caravanData = await context.GetAsync<CaravanData>();

            CaravanPosition pos = await _caravanService.GetPosition(context);

            LevelTrackRewardSettings levelRewardSettings = _gameData.Get<LevelTrackRewardSettings>(coreData);

            if (!pos.OnRoad() && pos.GetCurrentCity() == null)
            {
                await _serverCaravanService.EnterCity(context, levelRewardSettings.StartCityId, true);
            }

            if (coreData.Vars[TraderVars.Mult] < PlayMultConstants.MinMult)
            {
                coreData.Vars[TraderVars.Mult] = PlayMultConstants.MinMult;
            }

            bool didJustLevelUp = coreData.Level < 1;
            if (didJustLevelUp)
            {
                LevelTrackDifficultySettings diffSettings = _gameData.Get<LevelTrackDifficultySettings>(coreData);
                coreData.Level = 1;
                coreData.Currencies[CoreCurrencyTypes.Exp] = 0;
                coreData.Vars[TraderVars.ExpToLevelUp] = (int)diffSettings.GetExpToNextLevel(coreData.Level);
                coreData.SetNextHourlyUpdate();
            }

            if (!holdings.CaravanMembersOwned.HasBitIndex(levelRewardSettings.StartCaravanMemberId))
            {
                holdings.CaravanMembersOwned.SetBitIndex(levelRewardSettings.StartCaravanMemberId);
            }

            if (caravanData.CurrentMembers.Count < 1 && (didJustLevelUp || coreData.Level == 1))
            {
                await _caravanService.UpdateCaravanMembers(context, new List<long>() { levelRewardSettings.StartCaravanMemberId });
            }

            if (!holdings.SkinsOwned.HasBitIndex(levelRewardSettings.StartSkinTypeId))
            {
                holdings.SkinsOwned.SetBitIndex(levelRewardSettings.StartSkinTypeId);
            }

            if (caravanData.SkinTypeId == 0)
            {
                caravanData.SkinTypeId = levelRewardSettings.StartSkinTypeId;
            }

            List<Reward> rewards = await _calcAttributeService.CalcBaseAttributes(context, didJustLevelUp);

            await _calcAttributeService.CalcBuffs(context);
        }
    }
}


