using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.SpendHelpers;
using OxDb.SharedGame.Trader.CurrencySpend.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CurrencySpend.Services
{

    public interface ICurrencySpendService : IInjectable
    {
        ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, long spendLocationId, bool useCurrentCity);
        ValueTask<SpendCurrencyResponse> SpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request);
        ValueTask<SpendCurrencyCheckResult> CheckCanSpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request);
    }

    public class CurrencySpendService : ICurrencySpendService
    {

        protected IRewardService _rewardService = null;

        protected SetupDictionaryContainer<long, ISpendLocationHelper> _spendHelpers = new SetupDictionaryContainer<long, ISpendLocationHelper>();

        public async ValueTask<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, long spendLocationId, bool useCurrentCity)
        {

            if (!_spendHelpers.TryGetValue(spendLocationId, out ISpendLocationHelper helper))
            {
                return new FullSpendLocation();
            }

            return await helper.GetFullSpendLocation(lookup, useCurrentCity);
        }

        protected SpendCurrencyCheckResult SetSpendCurrencyResultState(SpendCurrencyCheckResult result, ESpendCurrencyCheckState state)
        {
            result.State = state;
            return result;
        }

        public async ValueTask<SpendCurrencyCheckResult> CheckCanSpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request)
        {

            SpendCurrencyCheckResult result = new SpendCurrencyCheckResult();
            result.FullLocation = await GetFullSpendLocation(lookup, request.SpendLocationId, request.UseCurrentCity);

            if (result.FullLocation == null)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.LocationDoesNotExist);
            }

            if (!result.FullLocation.IsValid)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.LocationIsNotValid);
            }

            result.SpendType = result.FullLocation.SpendTypes.FirstOrDefault(x => x.Index == request.SpendTypeIndex);

            if (result.SpendType == null)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.SpendTypeDoesNotExist);
            }

            if (result.SpendType.SpendCoreCurrencyTypeId != request.SpendCoreCurrencyTypeId)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.CurrencyTypeIsIncorrect);
            }

            if (result.SpendType.SpendQuantity != request.SpendQuantity)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.CurrencyQuantityIsIncorrect);
            }

            if (result.SpendType.SpendQuantity < 1)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.SpendQuantityMustBePositive);
            }

            if (result.SpendType.Rewards.Count < 1)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.NoSpendRewards);
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData.Currencies[result.SpendType.SpendCoreCurrencyTypeId] < result.SpendType.SpendQuantity)
            {
                return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.NotEnoughCurrency);
            }

            return SetSpendCurrencyResultState(result, ESpendCurrencyCheckState.Success);
        }

        public async ValueTask<SpendCurrencyResponse> SpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request)
        {

            SpendCurrencyResponse response = new SpendCurrencyResponse()
            {
                ExtraRewardArgs = request.ExtraRewardArgs,
            };

            SpendCurrencyCheckResult spendResult = await CheckCanSpendCurrency(lookup, request);

            response.State = spendResult.State;
            if (spendResult.State != ESpendCurrencyCheckState.Success)
            {
                return response;
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            RewardParams args = new RewardParams()
            {
                ExtraRewardArgs = request.ExtraRewardArgs,

            };

            List<IEffect> startEffects = spendResult.SpendType.Rewards.OfType<IEffect>().ToList();

            List<Reward> finalRewards = new List<Reward>();
            if (startEffects.Count == 1 && request.TargetEntityId > 0)
            {
                Reward rew = new Reward(startEffects[0]);
                rew.EntityId = request.TargetEntityId;
                rew.UniqueId = ++coreData.UniqueId;
                finalRewards.Add(rew);
            }

            if (finalRewards.Count < 1)
            {
                response.State = ESpendCurrencyCheckState.NoSpendRewards;
                return response;
            }
            if (await _rewardService.GiveRewards(lookup, startEffects, RewardSources.SpendCurrencyRewards, args))
            {
                response.State = ESpendCurrencyCheckState.Success;
            }
            else
            {
                response.State = ESpendCurrencyCheckState.FailedToGiveRewards;
            }

            if (response.State == ESpendCurrencyCheckState.Success)
            {
                response.Rewards.AddRange(_rewardService.CreateListFromList(RewardSources.SpendCurrencyRewards, request.SpendLocationId, finalRewards));
                Reward spendReward = new Reward()
                {
                    EntityTypeId = EntityTypes.CoreCurrency,
                    EntityId = spendResult.SpendType.SpendCoreCurrencyTypeId,
                    Quantity = -spendResult.SpendType.SpendQuantity
                };

                response.Rewards.AddRange(_rewardService.CreateListFromReward(RewardSources.SpendCurrencyCost, request.SpendLocationId, spendReward));
            }

            return response;

        }
    }
}
