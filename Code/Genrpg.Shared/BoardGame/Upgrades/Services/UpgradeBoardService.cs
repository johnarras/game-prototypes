using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.BoardGame.Upgrades.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.UserAbilities.Services;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserCoins.Settings;
using Genrpg.Shared.UserEnergy.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace Genrpg.Shared.BoardGame.Upgrades.Services
{
    public interface IUpgradeBoardService : IInjectable
    {
        int GetUpgradeCostPercent(IFilteredObject user, CoreUserData userDatam, int level);
        int GetStartUpgradeTier(IFilteredObject user, int boardLevel);
        int GetEndUpgradeTier(IFilteredObject user, int boardLevel);
        int GetUpgradeTierCount(IFilteredObject user, int boardLevel);

        int GetTileEffectValue(IFilteredObject user, BoardData boardData, long entityTypeId, long entityId, long baseQuantity);
        int GetMaxCoinStorage(IFilteredObject user, BoardData boardData, long userCoinTypeId);
        int GetCoinRewardMult(IFilteredObject user, BoardData boardData, long userCoinTypeId);
        double TokensPerUserCoin(IFilteredObject user);


        bool BoardIsComplete(IFilteredObject user, BoardData boardData, CoreUserData userData);

        UpgradeCosts GetUpgradeCosts(IFilteredObject user, CoreUserData userData, long tileTypeId, long currTierId);

        UpgradeCounts GetUpgradeCounts(IFilteredObject user, BoardData boardData);
        List<TileType> GetUpgradeableTilesForBoard(IFilteredObject user, BoardData boardData);
    }



    public class UpgradeBoardService : IUpgradeBoardService
    {
        private IGameData _gameData = null;


        public int GetUpgradeCostPercent(IFilteredObject user, CoreUserData userData, int level)
        {

            UpgradeBoardSettings upgradeSettings = _gameData.Get<UpgradeBoardSettings>(user);

            double upgradeDays = upgradeSettings.GetUpgradeDays(level);

            UserEnergySettings energySettings = _gameData.Get<UserEnergySettings>(user);

            double energyPerHour = energySettings.EnergyPerHour(level);

            double totalDicePerDay = upgradeSettings.FullDayEnergyCollectionHours * energyPerHour;

            double totalDice = totalDicePerDay * upgradeDays; // Total dice over the time

            long startUpgradeTier = GetStartUpgradeTier(user, level);

            double tierResources = 1 + startUpgradeTier * upgradeSettings.ExtraResourcesPerTier;

            // Multiply by the expected resources per die spent.
            double expectedResourcesCollected = totalDice * upgradeSettings.CurrencyMultPerDie * tierResources;

            // Get cost scaling for the upgrade tiers.
            IReadOnlyList<int> upgradeScales = upgradeSettings.GetUpgradeTierScaling();

            // Sum the upgrade scales.
            int totalScales = upgradeScales.Sum(x => x);

            // Base resources needed.
            double baseResourcesNeeded = totalScales * upgradeSettings.BaseTotalResourceCost;

            // Now divide the expected resources by the total resources needed to get the scaling.

            double resourceMult = expectedResourcesCollected / baseResourcesNeeded;

            // Now round to a nice number.

            int sizePower = (int)Math.Log10(resourceMult);

            if (sizePower > 2)
            {
                resourceMult /= Math.Pow(10, sizePower-2);
                resourceMult = (int)resourceMult;
            }

            resourceMult = resourceMult * 100;

            resourceMult /= 5;

            int upgradeCostPercent = (int)resourceMult;

            upgradeCostPercent *= 5;

            if (sizePower > 2)
            {
                upgradeCostPercent *= (int)Math.Pow(10, sizePower - 2);
            }


            userData.Vars.Set(UserVars.UpgradeCostPercent, upgradeCostPercent);

            return upgradeCostPercent;
        }


        public int GetStartUpgradeTier(IFilteredObject user, int boardLevel)
        {
            UpgradeBoardSettings settings = _gameData.Get<UpgradeBoardSettings>(user);
            int totalTiers = 1;

            for (int currLevel = 1; currLevel < boardLevel; currLevel++)
            {
                int currMaxTier = settings.GetUpgradeTiers(currLevel);
                totalTiers += currMaxTier;
                if (currMaxTier == settings.MaxTiers)
                {
                    totalTiers += (boardLevel - currLevel - 1) * settings.MaxTiers;
                    break;
                }
            }

            return totalTiers;
        }

        public int GetUpgradeTierCount(IFilteredObject user, int boardLevel)
        {
            UpgradeBoardSettings settings = _gameData.Get<UpgradeBoardSettings>(user);
            return settings.GetUpgradeTiers(boardLevel);
        }

        public int GetEndUpgradeTier(IFilteredObject user, int boardLevel)
        {
            return GetStartUpgradeTier(user, boardLevel) + GetUpgradeTierCount(user, boardLevel);
        }

        public int GetMaxCoinStorage(IFilteredObject user, BoardData boardData, long userCoinTypeId)
        {

            UserCoinType coinType = _gameData.Get<UserCoinSettings>(user).Get(userCoinTypeId);

            if (coinType == null || coinType.BaseStorage < 1)
            {
                return 0;
            }

            return GetTileEffectValue(user, boardData, EntityTypes.UserCoinMaxStorage, userCoinTypeId, coinType.BaseStorage);
        }

        public int GetCoinRewardMult (IFilteredObject user, BoardData boardData, long userCoinTypeId)
        {
            return GetTileEffectValue(user, boardData, EntityTypes.UserCoinRewardMult, userCoinTypeId, BoardGameConstants.BaseRewardMult);
        }

        public int GetTileEffectValue(IFilteredObject user, BoardData boardData, long entityTypeId, long entityId, long baseQuantity)
        {

            TileType tileType = _gameData.Get<TileTypeSettings>(user).GetEffectTileType(entityTypeId, entityId);

            if (tileType == null)
            {
                return (int)baseQuantity;
            }

            int effectQuantity = tileType.GetEffectQuantity(entityTypeId, entityId);

            int upgradeTier = boardData.GetTotalUpgradeTier(tileType.IdKey);

            return (int)baseQuantity + effectQuantity * upgradeTier; 
        }

        public double TokensPerUserCoin(IFilteredObject user)
        {

            UpgradeBoardSettings upgradeSettings = _gameData.Get<UpgradeBoardSettings>(user);

            long startUpgradeTier = GetStartUpgradeTier(user, user.Level);

            // Expect to spend X tokens per roll's worth of currencies
            double tokensPerRoll = upgradeSettings.TokensPerRoll;

            // Expected currencies per roll is the baseline * the start upgrade tier for this board.
            double currenciesPerRoll = startUpgradeTier * upgradeSettings.CurrenciesPerDie;

            // So the value is the product of these things.

            return tokensPerRoll * currenciesPerRoll;
        }


        public UpgradeCosts GetUpgradeCosts (IFilteredObject user, CoreUserData userData, long tileTypeId, long currTierId)
        {
            UpgradeCosts costs = new UpgradeCosts();


            UserCoinSettings coinSettings = _gameData.Get<UserCoinSettings>(user);

            TileType tileType = _gameData.Get<TileTypeSettings>(user).Get(tileTypeId);


            if (tileType == null || !tileType.CanUpgrade())
            {
                costs.ErrorMessage = "Cannot upgrade this TileType.";
                return costs;
            }

            long startTier = GetStartUpgradeTier(user, user.Level);
            long maxTier = GetEndUpgradeTier(user, user.Level);
            long tierCount = GetUpgradeTierCount(user, user.Level);

            if (currTierId >= tierCount)
            {
                costs.ErrorMessage = "This tile is fully upgraded.";
                return costs;
            }


            long costPercent = GetUpgradeCostPercent(user, userData, user.Level);

            costs.Reagents = new List<UpgradeReagent>();

            foreach (TileUpgradeReagent tileReagent in tileType.UpgradeReagents)
            {
                long newCost = tileReagent.Quantity * costPercent / 100;
                if (newCost > 0)
                {
                    UserCoinType ctype = coinSettings.Get(tileReagent.UserCoinTypeId);
                    if (ctype != null)
                    {
                        costs.Reagents.Add(new UpgradeReagent()
                        {
                            UserCoinTypeId = tileReagent.UserCoinTypeId,
                            RequiredQuantity = newCost,
                            CurrQuantity = userData.Coins.Get(tileReagent.UserCoinTypeId),
                            MissingQuantity = Math.Max(0, newCost-userData.Coins.Get(tileReagent.UserCoinTypeId)),
                        });
                    }
                }
            }

            if (costs.Reagents.Count < 1)
            {
                costs.ErrorMessage = "This tile has no upgrade formula.";
                return costs;
            }

            long missingCoinCount = costs.Reagents.Sum(x => x.MissingQuantity);

            double tokensPerCurrency = TokensPerUserCoin(user);

            costs.ExtraTokenCost = (long)Math.Ceiling(missingCoinCount * tokensPerCurrency);

            if (costs.ExtraTokenCost > userData.Coins.Get(UserCoinTypes.HardCurrency))
            {
                costs.ErrorMessage = "You need more " + _gameData.Get<UserCoinSettings>(user).GetName(UserCoinTypes.HardCurrency) + "s";
                costs.CanUpgradeNow = false;
                return costs;
            }

            costs.CanUpgradeNow = true;

            return costs;
        }

        public bool BoardIsComplete(IFilteredObject user, BoardData boardData, CoreUserData userData)
        {
            IReadOnlyList<TileType> allTileTypes = _gameData.Get<TileTypeSettings>(user).GetData();

            if (!boardData.IsOwnBoard())
            {
                return false;
            }

            long maxTier = GetUpgradeTierCount(user, user.Level);

            for (int i = 0; i < boardData.Tiles.GetLength(); i++)
            {
                TileType tileType = allTileTypes.FirstOrDefault(x => x.IdKey == boardData.Tiles.Get(i));

                if (tileType == null || !tileType.CanUpgrade())
                {
                    continue;
                }

                if (boardData.GetCurrentUpgradeTier(tileType.IdKey) < maxTier)
                {
                    return false;
                }
            }

            return true;
        }

        public List<TileType> GetUpgradeableTilesForBoard(IFilteredObject user, BoardData boardData)
        {
            TileTypeSettings settings = _gameData.Get<TileTypeSettings>(user);

            List<TileType> foundTileTypes = new List<TileType>();

            for (int i = 0; i < boardData.Tiles.GetLength(); i++)
            {
                TileType tileType = settings.Get(boardData.Tiles.Get(i));

                if (tileType == null || !tileType.CanUpgrade() || foundTileTypes.Contains(tileType))
                {
                    continue;
                }

                foundTileTypes.Add(tileType);
            }

            foundTileTypes = foundTileTypes.OrderBy(x => x.Name).ToList();

            return foundTileTypes;
        }

        public UpgradeCounts GetUpgradeCounts(IFilteredObject user, BoardData boardData)
        {
            UpgradeCounts counts = new UpgradeCounts();
            if (!boardData.IsOwnBoard())
            {
                counts.IsOwnBoard = false;
                return counts;
            }
            counts.IsOwnBoard = true;

            List<TileType> tileTypes = GetUpgradeableTilesForBoard(user, boardData);

            counts.UpgradeTileTypeCount = tileTypes.Count;
            counts.UpgradeTiers = boardData.UpgradeTierCount;
            counts.TotalUpgrades = counts.UpgradeTileTypeCount * counts.UpgradeTiers;

            foreach (TileType tileType in tileTypes)
            {
                counts.CurrUpgrades += (int)boardData.GetCurrentUpgradeTier(tileType.IdKey);
            }

            return counts;
        }
    }
}
