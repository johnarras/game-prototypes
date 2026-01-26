using Genrpg.RequestServer.Core;
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
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Holdings.PlayerData;
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
        private IServerCaravanService _serverCaravanService = null;

        public async Task UpdatePlayerOnLogin(WebContext context, bool onLogin)
        {
            CoreData CoreData = await context.GetAsync<CoreData>();

            List<Reward> newRewards = new List<Reward>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.core);

            NewPlayerBonusSettings newPlayerSettings = _gameData.Get<NewPlayerBonusSettings>(context.core);

            List<LevelTrackReward> levelRewards = _gameData.Get<LevelTrackRewardSettings>(context.core).GetData().Where(x => x.Level <= CoreData.Level).ToList();

            TraderStatSettings statSettings = _gameData.Get<TraderStatSettings>(context.core);

            TraderStatData statData = await context.GetAsync<TraderStatData>();

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            CaravanData caravanData = await context.GetAsync<CaravanData>();

            CaravanPosition pos = _caravanService.GetPosition(context.core);

            if (!pos.OnRoad() && pos.GetCurrentCity() == null)
            {

                await _serverCaravanService.EnterCity(context, newPlayerSettings.StartCityId, true);
            }

            if (CoreData.Vars[TraderVars.Mult] < PlayMultConstants.MinMult)
            {
                CoreData.Vars[TraderVars.Mult] = PlayMultConstants.MinMult;
            }

            foreach (IReward rew in newPlayerSettings.GetData())
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    if (statData.Stats[rew.EntityId].Base < rew.Quantity)
                    {
                        statData.Stats[rew.EntityId].Base = (int)rew.Quantity;
                    }
                }
                else if (CoreData.Level < 1 && rew.EntityTypeId == EntityTypes.CoreCurrency)
                {
                    CoreData.Currencies[rew.EntityId] = rew.Quantity;
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    _animalService.AddAnimalToHoldings(context.core, await context.GetAsync<HoldingsData>(), rew.EntityId);
                }

            }

            foreach (IReward rew in levelRewards)
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    if (statData.Stats[rew.EntityId].Base < rew.Quantity)
                    {
                        statData.Stats[rew.EntityId].Base = (int)rew.Quantity;
                    }
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    _animalService.AddAnimalToHoldings(context.core, await context.GetAsync<HoldingsData>(), rew.EntityId);
                }
            }
            if (context.core.Level < 1)
            {
                context.core.Level = 1;
                context.core.Vars[TraderVars.Exp] = 0;
                context.core.SetNextHourlyUpdate();

                List<NewPlayerBonus> startTradeGoods = newPlayerSettings.GetData().Where(x => x.EntityTypeId == EntityTypes.TradeGood).ToList();

                foreach (IReward rew in startTradeGoods)
                {
                    _tradeGoodService.AddTradeGoodToCaravan(CoreData, caravanData, statData, rew.EntityId);
                }
            }

            if (caravanData.Animals.Count < 1)
            {

                IReadOnlyList<AnimalType> animals = _gameData.Get<AnimalTypeSettings>(context.core).GetData();

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
                    chosenAnimal = animals.OrderBy(x => x.Price).FirstOrDefault();
                }
                else
                {
                    chosenAnimal = ownedAnimals.OrderBy(x => x.Price).FirstOrDefault();
                }

                _caravanService.AddAnimalToCaravan(CoreData, caravanData, holdings, statData, chosenAnimal.IdKey, true);
            }

            _caravanService.UpdateTravelStatsFromCaravan(CoreData, caravanData, statData);
        }
    }
}


