using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.CurrencySpend.SpendHelpers;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Trader.CurrencySpend.Services
{

    public interface ICurrencySpendService : IInjectable
    {
        Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, long spendLocationId, bool useCurrentCity);
        Task<SpendCurrencyResponse> SpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request);
        Task<SpendCurrencyCheckResult> CheckCanSpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request);
    }

    public class CurrencySpendService : ICurrencySpendService
    {

        protected IRewardService _rewardService = null;

        protected SetupDictionaryContainer<long, ISpendLocationHelper> _spendHelpers = new SetupDictionaryContainer<long, ISpendLocationHelper>();

        public async Task<FullSpendLocation> GetFullSpendLocation(IUnitDataLookup lookup, long spendLocationId, bool useCurrentCity)
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

        public async Task<SpendCurrencyCheckResult> CheckCanSpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request)
        {

            SpendCurrencyCheckResult result = new SpendCurrencyCheckResult();
            result.FullLocation = await GetFullSpendLocation(lookup, request.SpendLocationId, request.UseCurrentCity);

            if (result.FullLocation == null)
            {
                return SetSpendCurrencyResultState(result ,ESpendCurrencyCheckState.LocationDoesNotExist);
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

        public async Task<SpendCurrencyResponse> SpendCurrency(IUnitDataLookup lookup, SpendCurrencyRequest request)
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

            List<IEffect> rewards = spendResult.SpendType.Rewards.Cast<IEffect>().ToList(); 

            if (rewards.Count == 1 && request.TargetEntityId > 0)
            {
                Reward rew = new Reward(rewards[0]);
                rew.EntityId = request.TargetEntityId;
                rew.UniqueId = ++coreData.UniqueId;
                rewards = new List<IEffect>() { rew };
            }

            if (rewards.Count < 1)
            {
                response.State = ESpendCurrencyCheckState.NoSpendRewards;
                return response;
            }
            if (await _rewardService.GiveRewards(lookup, rewards, args))
            {
                response.State = ESpendCurrencyCheckState.Success;   
            }
            else
            {
                response.State = ESpendCurrencyCheckState.FailedToGiveRewards;
            }

            if (response.State == ESpendCurrencyCheckState.Success)
            {
                foreach (IEffect eff in rewards)
                {
                    response.Rewards.Add(new Reward(eff));
                }

                coreData.Currencies[spendResult.SpendType.SpendCoreCurrencyTypeId] -= spendResult.SpendType.SpendQuantity;
                response.Rewards.Add(new Reward()
                {
                    EntityTypeId = EntityTypes.CoreCurrency,
                    EntityId = spendResult.SpendType.SpendCoreCurrencyTypeId,
                    Quantity = -spendResult.SpendType.SpendQuantity
                });
            }

            return response;

        }
    }
}
