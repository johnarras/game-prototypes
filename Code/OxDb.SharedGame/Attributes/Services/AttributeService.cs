using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Attributes.Constants;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Attributes.Services
{
    public interface IAttributeService : IInitializable
    {
        Task UpdateBuffsAndDebuffs(IUnitDataLookup lookup);

        Task AddBuff(IUnitDataLookup lookup, long gameplayBuffId, long seconds);
        Task<long> GetBuffSeconds(IUnitDataLookup lookup, long gameplayBuffId);

        Task AddDebuff(IUnitDataLookup lookup, long gameplayDebuffId, long daysUntilDispelled);
        Task<long> GetDebuffDays(IUnitDataLookup lookup, long gameplayDebuffId);

        Task CheckBuffs(IUnitDataLookup lookup, bool forceRecalc);
        Task AddDebuffDaysPlayed(IUnitDataLookup lookup, long daysAdded);

        Task<long> GetQuantity(IUnitDataLookup lookup, long entityTypeId, long entityId);
        Task<long> GetQuantity(IUnitDataLookup lookup, EAttributeCategories category, EAttributeValIndex index, long entityId);
        Task<bool> GiveReward(IUnitDataLookup lookup, long entityTypeId, long entityId, long quantity);
        Task<bool> GiveReward(IUnitDataLookup lookup, EAttributeCategories category, EAttributeValIndex index, long entityId, long quantity);

        Task<bool> ApplyAttributeIndexEffect(IUnitDataLookup lookup, IEffect effect, EAttributeValIndex index);
        bool EntityTypeHasValIndex(long entityTypeId, EAttributeValIndex index);
    }


    public class EntityToAttributeMapping
    {
        public EAttributeCategories Category { get; set; }
        public EAttributeValIndex Index { get; set; }
    }

    public class AttributeService : IAttributeService
    {

        private IGameData _gameData = null;
        private ICalcAttributeService _calcAttributeService = null;



        Dictionary<long, EntityToAttributeMapping> _mappingDict = new Dictionary<long, EntityToAttributeMapping>();

        public async Task Initialize(CancellationToken token)
        {
            _mappingDict.Clear();

            _mappingDict[EntityTypes.BaseGameplayStat] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.Stats,
                Index = EAttributeValIndex.Base,
            };
            _mappingDict[EntityTypes.BonusGameplayStat] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.Stats,
                Index = EAttributeValIndex.Bonus,
            };
            _mappingDict[EntityTypes.GameplayStatBuff] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.Stats,
                Index = EAttributeValIndex.Buff,
            };

            _mappingDict[EntityTypes.BaseCurrencyRegen] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyRegen,
                Index = EAttributeValIndex.Base,
            };
            _mappingDict[EntityTypes.BonusCurrencyRegen] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyRegen,
                Index = EAttributeValIndex.Bonus,
            };
            _mappingDict[EntityTypes.CurrencyRegenBuff] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyRegen,
                Index = EAttributeValIndex.Buff,
            };

            _mappingDict[EntityTypes.BaseCurrencyStorage] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyStorage,
                Index = EAttributeValIndex.Base,
            };
            _mappingDict[EntityTypes.BonusCurrencyStorage] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyStorage,
                Index = EAttributeValIndex.Bonus,
            };
            _mappingDict[EntityTypes.CurrencyStorageBuff] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.CurrencyStorage,
                Index = EAttributeValIndex.Buff,
            };

            _mappingDict[EntityTypes.BaseTravelDayCurrency] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.TravelDayCurrency,
                Index = EAttributeValIndex.Base,
            };

            _mappingDict[EntityTypes.BonusTravelDayCurrency] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.TravelDayCurrency,
                Index = EAttributeValIndex.Bonus,
            };
            _mappingDict[EntityTypes.TravelDayCurrencyBuff] = new EntityToAttributeMapping()
            {
                Category = EAttributeCategories.TravelDayCurrency,
                Index = EAttributeValIndex.Buff,
            };


        }

        public async Task AddBuff(IUnitDataLookup lookup, long gameplayBuffId, long seconds)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();

            GameplayBuff currBuff = _gameData.Get<GameplayBuffSettings>(coreData).Get(gameplayBuffId);

            if (currBuff == null)
            {
                return;
            }

            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

            GameplayBuffStatus buffStatus = attributeData.Buffs[gameplayBuffId];

            DateTime startTime = DateTime.UtcNow;

            if (buffStatus.EndTime > startTime)
            {
                startTime = buffStatus.EndTime;
            }

            buffStatus.EndTime = startTime.AddSeconds(seconds);

            await UpdateBuffsAndDebuffs(lookup);
        }

        public async Task AddDebuff(IUnitDataLookup lookup, long gameplayDebuffId, long daysUntilDispelled)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();
            GameplayDebuff debuff = _gameData.Get<GameplayDebuffSettings>(coreData).Get(gameplayDebuffId);

            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

            GameplayDebuffStatus debuffStatus = attributeData.Debuffs[gameplayDebuffId];

            if (daysUntilDispelled > 0)
            {
                long playCount = coreData.Vars[TraderVars.DebuffDaysPlayed];

                long startPlayCount = Math.Max(debuffStatus.EndDebuffPlayCount, playCount);

                long endPlayCount = startPlayCount + daysUntilDispelled;

                debuffStatus.EndDebuffPlayCount = (int)endPlayCount;
            }
            else
            {
                debuffStatus.EndDebuffPlayCount = 0;
            }

            await UpdateBuffsAndDebuffs(lookup);
        }



        public virtual async Task UpdateBuffsAndDebuffs(IUnitDataLookup lookup)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            AttributesData attributeData = await lookup.GetAsync<AttributesData>();
            GameplayBuffSettings buffSettings = _gameData.Get<GameplayBuffSettings>(coreData);
            GameplayDebuffSettings debuffSettings = _gameData.Get<GameplayDebuffSettings>(coreData);
            GameplayStatSettings statSettings = _gameData.Get<GameplayStatSettings>(coreData);

            attributeData.ResetBuffs();

            int buffBits = 0;

            DateTime nowTime = DateTime.UtcNow;
            DateTime nextBuffEndsTime = DateTime.MinValue;

            foreach (GameplayBuff buff in buffSettings.GetData())
            {
                GameplayBuffStatus status = attributeData.Buffs[buff.IdKey];

                if (status.EndTime <= nowTime)
                {
                    status.EndTime = DateTime.MinValue;
                }
                else
                {
                    if (nextBuffEndsTime == DateTime.MinValue || nextBuffEndsTime > status.EndTime)
                    {
                        nextBuffEndsTime = status.EndTime;
                    }
                    buffBits |= (int)(1 << (int)buff.IdKey);
                }
            }

            coreData.Vars[TraderVars.BuffBits] = (int)buffBits;
            if (buffBits != 0)
            {
                coreData.NextBuffEndsTime = nextBuffEndsTime;
            }
            else
            {
                coreData.NextBuffEndsTime = DateTime.MinValue;
            }

            int debuffBits = 0;
            int currDebuffDaysPlayed = coreData.Vars[TraderVars.DebuffDaysPlayed];

            int nextDebuffEndPlayCount = 0;

            foreach (GameplayDebuff debuff in debuffSettings.GetData())
            {
                GameplayDebuffStatus status = attributeData.Debuffs[debuff.IdKey];

                if (status.EndDebuffPlayCount <= currDebuffDaysPlayed)
                {
                    status.EndDebuffPlayCount = 0;
                }
                else
                {
                    if (nextDebuffEndPlayCount == 0 || status.EndDebuffPlayCount < nextDebuffEndPlayCount)
                    {
                        nextDebuffEndPlayCount = status.EndDebuffPlayCount;
                    }
                    debuffBits |= (int)(1 << (int)debuff.IdKey);
                }
            }

            coreData.Vars[TraderVars.DebuffBits] = debuffBits;
            coreData.Vars[TraderVars.NextDebuffEndsDay] = nextDebuffEndPlayCount;

            await _calcAttributeService.CalcBuffs(lookup);
        }

        public async Task CheckBuffs(IUnitDataLookup lookup, bool forceRecalc)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            if (forceRecalc || (coreData.Vars[TraderVars.BuffBits] != 0 && coreData.NextBuffEndsTime <= DateTime.UtcNow))
            {
                await UpdateBuffsAndDebuffs(lookup);
            }
        }

        public virtual async Task AddDebuffDaysPlayed(IUnitDataLookup lookup, long debuffDaysAdded)
        {

            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (debuffDaysAdded == 0 || coreData.Vars[TraderVars.DebuffBits] == 0)
            {
                return;
            }

            coreData.Vars.Add(TraderVars.DebuffDaysPlayed, (int)debuffDaysAdded);

            // If this doesn't exceed the next debuff ends day, we can just bail out quickly.
            if (coreData.Vars[TraderVars.DebuffDaysPlayed] < coreData.Vars[TraderVars.NextDebuffEndsDay])
            {
                return;
            }

            await UpdateBuffsAndDebuffs(lookup);
        }

        public async Task<long> GetBuffSeconds(IUnitDataLookup lookup, long gameplayBuffId)
        {

            AttributesData attributeData = await lookup.GetAsync<AttributesData>();
            return (long)Math.Max(0, (attributeData.Buffs[gameplayBuffId].EndTime - DateTime.UtcNow).TotalSeconds);
        }

        public async Task<long> GetDebuffDays(IUnitDataLookup lookup, long gameplayDebuffId)
        {
            AttributesData attributeData = await lookup.GetAsync<AttributesData>();
            CoreData coreData = await lookup.GetAsync<CoreData>();
            return Math.Max(0, attributeData.Debuffs[gameplayDebuffId].EndDebuffPlayCount - coreData.Vars[TraderVars.DebuffDaysPlayed]);
        }

        protected AttributeStatus GetStatus(AttributesData attributeData, long entityTypeId, long entityId)
        {
            if (_mappingDict.TryGetValue(entityTypeId, out EntityToAttributeMapping mapping))
            {
                return attributeData.GetStatus(mapping.Category, entityId);
            }
            return null;
        }

        protected async Task<AttributeStatus> GetStatus(IUnitDataLookup lookup, EAttributeCategories category, long entityId)
        {
            AttributesData attributeData = await lookup.GetAsync<AttributesData>();

            return attributeData.GetStatus(category, entityId);
        }

        public async Task<long> GetQuantity(IUnitDataLookup lookup, long entityTypeId, long entityId)
        {
            if (_mappingDict.TryGetValue(entityTypeId, out EntityToAttributeMapping mapping))
            {
                return await GetQuantity(lookup, mapping.Category, mapping.Index, entityId);
            }
            return 0;
        }

        public async Task<long> GetQuantity(IUnitDataLookup lookup, EAttributeCategories category, EAttributeValIndex index, long entityId)
        {
            AttributeStatus status = await GetStatus(lookup, category, entityId);

            return status.GetQuantity(index);
        }

        public async Task<bool> GiveReward(IUnitDataLookup lookup, long entityTypeId, long entityId, long quantity)
        {
            if (_mappingDict.TryGetValue(entityTypeId, out EntityToAttributeMapping mapping))
            {
                return await GiveReward(lookup, mapping.Category, mapping.Index, entityId, quantity);
            }
            return false;
        }

        public async Task<bool> GiveReward(IUnitDataLookup lookup, EAttributeCategories category, EAttributeValIndex index, long entityId, long quantity)
        {
            AttributeStatus status = await GetStatus(lookup, category, entityId);

            return status.GiveReward(index, quantity);
        }

        public bool EntityTypeHasValIndex(long entityTypeId, EAttributeValIndex index)
        {

            if (_mappingDict.TryGetValue(entityTypeId, out EntityToAttributeMapping mapping))
            {
                return mapping.Index == index;
            }
            return false;
        }

        public async Task<bool> ApplyAttributeIndexEffect(IUnitDataLookup lookup, IEffect effect, EAttributeValIndex index)
        {
            if (!EntityTypeHasValIndex(effect.EntityTypeId, index))
            {
                return false;
            }

            return await GiveReward(lookup, effect.EntityTypeId, effect.EntityId, effect.Quantity);
        }
    }
}
