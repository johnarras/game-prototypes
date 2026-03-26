using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.LevelTrack.Services;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.PlayMultiplier.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.CaravanMembers.Services;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.TradeGoods.Services;

namespace Genrpg.RequestServer.Trader.NewPlayer
{

    public interface ITraderNewPlayerService : IInjectable
    {
        Task UpdatePlayerOnLogin(WebContext context, bool onLogin);
    }

    public class TraderNewPlayerService : ITraderNewPlayerService
    {

        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;
        private ITradeGoodService _tradeGoodService = null;
        private ICaravanMemberService _CaravanMemberService = null;
        private IServerCaravanService _serverCaravanService = null;
        private IAttributeService _attributeService = null;
        private IServerLevelTrackService _levelTrackService = null;

        public async Task UpdatePlayerOnLogin(WebContext context, bool onLogin)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            List<Reward> newRewards = new List<Reward>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(coreData);

            GameplayStatSettings statSettings = _gameData.Get<GameplayStatSettings>(coreData);

            AttributeData attributeData = await context.GetAsync<AttributeData>();

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            CaravanData caravanData = await context.GetAsync<CaravanData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

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

            List<Reward> rewards = await _levelTrackService.GiveLevelTrackRewards(context, didJustLevelUp);

            if (caravanData.CurrentMembers.Count < 1)
            {
                IReadOnlyList<CaravanMember> CaravanMembers = _gameData.Get<CaravanMemberSettings>(coreData).GetData();

                List<CaravanMember> ownedCaravanMembers = new List<CaravanMember>();

                foreach (CaravanMember CaravanMember in CaravanMembers)
                {
                    if (holdings.CaravanMembersOwned.HasBitIndex(CaravanMember.IdKey))
                    {
                        ownedCaravanMembers.Add(CaravanMember);
                    }
                }

                CaravanMember chosenCaravanMember = null;
                if (ownedCaravanMembers.Count < 1)
                {
                    chosenCaravanMember = CaravanMembers.OrderBy(x => x.Price).FirstOrDefault();
                }
                else
                {
                    chosenCaravanMember = ownedCaravanMembers.OrderBy(x => x.Price).FirstOrDefault();
                }

                await _caravanService.AddMemberToCaravan(context, chosenCaravanMember.IdKey, true);
            }

            if (caravanData.SkinTypeId == 0)
            {
                caravanData.SkinTypeId = 1;
            }


            await _caravanService.CalcCoreTravelStats(context);
        }
    }
}


