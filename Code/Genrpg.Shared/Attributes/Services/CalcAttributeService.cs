using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Attributes.Services
{
    public interface ICalcAttributeService : IInitializable
    {
        Task CalcBuffs(IUnitDataLookup lookup);
        Task<List<Reward>> CalcBaseAttributes(IUnitDataLookup lookup, bool didJustLevelUp);
        Task<List<Reward>> CalcAllAttributes(IUnitDataLookup lookup, bool didJustLevelUp);
    }

    public class CalcAttributeService : ICalcAttributeService
    {

        private IGameData _gameData = null;
        private ITraderMapService _traderMapService = null;
        private IAttributeService _attributeService = null;
        protected IRewardService _rewardService = null;


        private List<GameplayStatToTraderVarMapper> _statMappers = new List<GameplayStatToTraderVarMapper>();
        public async Task Initialize(CancellationToken token)
        {
            // Explicit list rather than reflection since this gets done at runtime.
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.MaxSize, TraderVars.MaxSize));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.MaxInventory, TraderVars.MaxInventory));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.Luck, TraderVars.Luck));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.Searching, TraderVars.Searching));
            _statMappers.Add(new GameplayStatToTraderVarMapper(GameplayStats.BonusSpeed, TraderVars.BonusSpeedPerDie));
            await Task.CompletedTask;
        }

        public virtual async Task<List<Reward>> CalcAllAttributes(IUnitDataLookup lookup, bool didJustLevelUp)
        {
            List<Reward> baseRewards = await CalcBaseAttributes(lookup, didJustLevelUp);
            await CalcBuffs(lookup);
            return baseRewards;
        }

        public virtual async Task CalcBuffs(IUnitDataLookup lookup)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            CaravanMemberSettings memberSettings = _gameData.Get<CaravanMemberSettings>(coreData);
            AttributeData attributeData = await lookup.GetAsync<AttributeData>();
            CaravanData caravanData = await lookup.GetAsync<CaravanData>();

            attributeData.ResetBuffs();

            await UpdateBuffsFromBitList<GameplayBuffSettings, GameplayBuff, Effect>(lookup, coreData, coreData.Vars[TraderVars.BuffBits]);
            await UpdateBuffsFromBitList<GameplayDebuffSettings, GameplayDebuff, Effect>(lookup, coreData, coreData.Vars[TraderVars.DebuffBits]);


            int baseDiceSpeed = 0;
            bool didSetDiceSpeed = false;
            foreach (CurrentCaravanMember currentMember in caravanData.CurrentMembers)
            {
                CaravanMember caravanMember = memberSettings.Get(currentMember.CaravanMemberId);

                if (caravanMember != null)
                {
                    foreach (Effect effect in caravanMember.Effects)
                    {
                        await _attributeService.ApplyAttributeIndexEffect(lookup, effect, EAttributeValIndex.Buff);
                    }

                    if (caravanMember.Speed == 0)
                    {
                        continue;
                    }

                    if (baseDiceSpeed == 0)
                    {
                        baseDiceSpeed = caravanMember.Speed;
                    }
                    else if (caravanMember.Speed < baseDiceSpeed)
                    {
                        baseDiceSpeed = caravanMember.Speed;
                    }
                    didSetDiceSpeed = true;
                }
            }

            if (!didSetDiceSpeed)
            {
                baseDiceSpeed = 0;
            }

            coreData.Vars[TraderVars.BaseDiceSpeed] = baseDiceSpeed;

            foreach (GameplayStatToTraderVarMapper mapper in _statMappers)
            {
                coreData.Vars[mapper.ToTraderVarId] = (int)attributeData.GetQuantity(EAttributeCategories.Stats, EAttributeValIndex.Total, mapper.FromGameplayStatId);
            }

            coreData.TravelDayCurrencies.Clear();

            for (int c = 0; c < attributeData.TravelDayCurrencies.Count(); c++)
            {
                long total = attributeData.TravelDayCurrencies[c].Total();

                if (total != 0)
                {
                    coreData.TravelDayCurrencies[c] = (int)total;
                }
            }

            int sizeUsed = 0;
            foreach (CurrentCaravanMember currentMember in caravanData.CurrentMembers)
            {
                CaravanMember member = memberSettings.Get(currentMember.CaravanMemberId);

                sizeUsed += member.Size;
            }

            coreData.Vars[TraderVars.SizeUsed] = sizeUsed;
            coreData.Vars[TraderVars.InventoryUsed] = caravanData.TradeGoods.Count;
        }



        protected async Task UpdateBuffsFromBitList<TParent, TChild, TEffect>(IUnitDataLookup lookup, CoreData coreData, long memberBits)
            where TParent : ParentSettings<TChild>
            where TChild : ChildSettings, IId, IEffectList<TEffect>, new()
            where TEffect : class, IEffect
        {

            IReadOnlyList<TChild> children = _gameData.Get<TParent>(coreData).GetData();

            foreach (TChild child in children)
            {
                if (FlagUtils.HasBitIndex(memberBits, child.IdKey))
                {
                    foreach (TEffect effect in child.Effects)
                    {
                        await _attributeService.ApplyAttributeIndexEffect(lookup, effect, EAttributeValIndex.Buff);
                    }
                }
            }
        }


        protected List<long> GetExcludedRepeatRewards()
        {
            return new List<long>() { EntityTypes.CoreCurrency, EntityTypes.TradeGood };
        }

        public async Task<List<Reward>> CalcBaseAttributes(IUnitDataLookup lookup, bool didJustLevelup)
        {

            List<Reward> retval = new List<Reward>();
            CoreData coreData = await lookup.GetAsync<CoreData>();

            LevelTrackRewardSettings rewardSettings = _gameData.Get<LevelTrackRewardSettings>(coreData);

            List<LevelTrackReward> rewards = rewardSettings.GetData().Where(x => x.Level <= coreData.Level).OrderBy(x => x.Level).ToList();

            List<LevelTrackReward> permRewards = rewards.Where(x => !GetExcludedRepeatRewards().Contains(x.EntityTypeId)).ToList();

            List<LevelTrackReward> oneTimeRewards = rewards.Except(permRewards).ToList();


            AttributeData attributeData = await lookup.GetAsync<AttributeData>();

            attributeData.ResetBase();

            if (didJustLevelup)
            {
                oneTimeRewards = oneTimeRewards.Where(x => x.Level == coreData.Level).ToList();
            }
            else
            {
                oneTimeRewards.Clear();
            }

            List<LevelTrackReward> finalRewards = permRewards.Concat(oneTimeRewards).ToList();


            foreach (LevelTrackReward rew in finalRewards)
            {
                await _attributeService.ApplyAttributeIndexEffect(lookup, rew, EAttributeValIndex.Base);
            }

            return retval;
        }
    }
}
