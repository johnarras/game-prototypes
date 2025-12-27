using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Trader.Travel.Entities;
using Genrpg.RequestServer.Trader.Travel.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.NewPlayers.Settings;
using Genrpg.Shared.PlayMultiplier.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Animals.Services;
using Genrpg.Shared.Trader.Animals.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Roads.Settings;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;
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
        private IAnimalService _animalService = null;
        private ICaravanPositionService _positionService = null;

        public async Task UpdatePlayerOnLogin(WebContext context, bool onLogin)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            List<Reward> newRewards = new List<Reward>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.user);

            NewPlayerBonusSettings newPlayerSettings = _gameData.Get<NewPlayerBonusSettings>(context.user);

            List<LevelTrackReward> levelRewards = _gameData.Get<LevelTrackRewardSettings>(context.user).GetData().Where(x => x.Level <= userData.Level).ToList();

            TraderStatSettings statSettings = _gameData.Get<TraderStatSettings>(context.user);

            TraderStatData statData = await context.GetAsync<TraderStatData>();

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            CaravanData caravanData = await context.GetAsync<CaravanData>();

            foreach (IReward rew in newPlayerSettings.GetData())
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    statData.Stats.Get(rew.EntityId).RaiseBaseToValue(rew.Quantity);
                }
                else if (userData.Level < 1 && rew.EntityTypeId == EntityTypes.CoreCurrency)
                {
                    userData.Currencies.Set(rew.EntityId, rew.Quantity);
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    _animalService.AddAnimalToHoldings(context.user, await context.GetAsync<HoldingsData>(), rew.EntityId);
                }

            }

            foreach (IReward rew in levelRewards)
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    statData.Stats.Get(rew.EntityId).RaiseBaseToValue(rew.Quantity);
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    _animalService.AddAnimalToHoldings(context.user, await context.GetAsync<HoldingsData>(), rew.EntityId);
                }
            }
            if (context.user.Level < 1)
            {
                context.user.Level = 1;
                context.user.Exp = 0;
                context.user.SetNextHourlyUpdate();

                List<NewPlayerBonus> startTradeGoods = newPlayerSettings.GetData().Where(x => x.EntityTypeId == EntityTypes.TradeGood).ToList();

                foreach (IReward rew in startTradeGoods)
                {
                    _tradeGoodService.AddTradeGoodToCaravan(userData, caravanData, statData, rew.EntityId);
                }
            }

            if (caravanData.Animals.Count < 1)
            {

                IReadOnlyList<AnimalType> animals = _gameData.Get<AnimalTypeSettings>(context.user).GetData();

                List<AnimalType> ownedAnimals = new List<AnimalType>();

                foreach (AnimalType animal in animals)
                {
                    if (holdings.AnimalsOwned.HasBit(animal.IdKey))
                    {
                        ownedAnimals.Add(animal);
                    }
                }

                AnimalType chosenAnimal = null;
                if (ownedAnimals.Count < 1)
                {
                    chosenAnimal = animals.OrderBy(x => x.Cost).FirstOrDefault();
                }
                else
                {
                    chosenAnimal = ownedAnimals.OrderBy(x => x.Cost).FirstOrDefault();
                }

                _caravanService.AddAnimalToCaravan(userData, caravanData, holdings, statData, chosenAnimal.IdKey, true);
            }



            CaravanPosition pos = context.user.GetPosition();

            City city = _gameData.Get<CitySettings>(context.user).Get(pos.CityId);
            Road road = _gameData.Get<RoadSettings>(context.user).Get(pos.RoadId);

            if (road == null && city == null)
            {
                EnterCityArgs args = new EnterCityArgs()
                {
                    CityId = newPlayerSettings.StartCityId,
                    Force = true,
                };
                await _positionService.EnterCity(context, args);
            }

            if (userData.Mult < PlayMultConstants.MinMult)
            {
                userData.Mult = PlayMultConstants.MinMult;
            }

            _caravanService.UpdateCoreStatsFromCaravan(userData, caravanData, statData);
        }
    }
}


