using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;

namespace Genrpg.Shared.Attributes.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class AttributeData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public AttributeCollection Stats { get; set; } = new AttributeCollection();
        [Key(2)] public AttributeCollection CurrencyRegen { get; set; } = new AttributeCollection();
        [Key(3)] public AttributeCollection CurrencyStorage { get; set; } = new AttributeCollection();

        [Key(4)] public GameplayBuffCollection Buffs { get; set; } = new GameplayBuffCollection();

        [Key(5)] public GameplayDebuffCollection Debuffs { get; set; } = new GameplayDebuffCollection();

        [Key(6)] public AttributeCollection TravelDayCurrencies { get; set; } = new AttributeCollection();


        public AttributeCollection GetAttributeCollection(EAttributeCategories category)
        {
            if (category == EAttributeCategories.Stats)
            {
                return Stats;
            }
            else if (category == EAttributeCategories.CurrencyRegen)
            {
                return CurrencyRegen;
            }
            else if (category == EAttributeCategories.CurrencyStorage)
            {
                return CurrencyStorage;
            }
            else if (category == EAttributeCategories.TravelDayCurrency)
            {
                return TravelDayCurrencies;
            }
            return null;
        }

        public AttributeStatus GetStatus(EAttributeCategories category, long entityId)
        {
            AttributeCollection collection = GetAttributeCollection(category);

            AttributeStatus status = collection[entityId];

            return status;

        }

        public void ResetBuffs()
        {

            foreach (EAttributeCategories category in Enum.GetValues(typeof(EAttributeCategories)))
            {

                AttributeCollection collection = GetAttributeCollection(category);

                for (int i = 0; i < collection.Count(); i++)
                {
                    collection[i].Buff = 0;
                }
            }
        }


        public void ResetBase()
        {

            foreach (EAttributeCategories category in Enum.GetValues(typeof(EAttributeCategories)))
            {

                AttributeCollection collection = GetAttributeCollection(category);

                for (int i = 0; i < collection.Count(); i++)
                {
                    collection[i].Base = 0;
                }
            }
        }

        public long GetQuantity(EAttributeCategories category, EAttributeValIndex statVal, long entityId)
        {
            AttributeCollection collection = GetAttributeCollection(category);

            AttributeStatus status = collection[entityId];

            if (statVal == EAttributeValIndex.Base)
            {
                return status.Base;
            }
            else if (statVal == EAttributeValIndex.Bonus)
            {
                return status.Bonus;
            }
            else if (statVal == EAttributeValIndex.Buff)
            {
                return status.Buff;
            }
            else if (statVal == EAttributeValIndex.Total)
            {
                return status.Total();
            }
            return 0;
        }
    }

    [MessagePackObject]
    public class AttributeCollection : BaseSmallIdObjectCollection<AttributeStatus>
    {
        [Key(0)] public AttributeStatus[] Data { get => _data; set => _data = value; }
        protected override AttributeStatus InternalAdd(AttributeStatus first, AttributeStatus second)
        {
            throw new NotImplementedException("Cannot add two GameStatStatuses together");
        }

        protected override bool IsDefault(AttributeStatus t)
        {
            return t == default(AttributeStatus);
        }
    }

    [MessagePackObject]
    public class AttributeStatus
    {
        [Key(0)] public int Base { get; set; }
        [Key(1)] public int Bonus { get; set; }

        [Key(2)] public int Buff { get; set; }

        public int Total() { return Base + Bonus + Buff; }


        public long GetQuantity(EAttributeValIndex index)
        {
            if (index == EAttributeValIndex.Base)
            {
                return Base;
            }
            else if (index == EAttributeValIndex.Bonus)
            {
                return Bonus;
            }
            else if (index == EAttributeValIndex.Buff)
            {
                return Buff;
            }
            else if (index == EAttributeValIndex.Total)
            {
                return Total();
            }
            return 0;
        }

        public bool GiveReward(EAttributeValIndex index, long quantity)
        {
            if (index == EAttributeValIndex.Base)
            {
                Base += (int)quantity;
            }
            else if (index == EAttributeValIndex.Bonus)
            {
                Bonus += (int)quantity;
            }
            else if (index == EAttributeValIndex.Buff)
            {
                Buff += (int)quantity;
            }
            else
            {
                return false;
            }
            return true;
        }

    }

    [MessagePackObject]
    public class GameplayDebuffCollection : BaseSmallIdObjectCollection<GameplayDebuffStatus>
    {
        [Key(0)] public GameplayDebuffStatus[] Data { get => _data; set => _data = value; }
        protected override GameplayDebuffStatus InternalAdd(GameplayDebuffStatus first, GameplayDebuffStatus second)
        {
            throw new NotImplementedException("Cannot add two TDebuffStatuses together");
        }

        protected override bool IsDefault(GameplayDebuffStatus t)
        {
            return t == default(GameplayDebuffStatus);
        }
    }

    [MessagePackObject]
    public class GameplayDebuffStatus
    {
        [Key(0)] public int EndDebuffPlayCount { get; set; }
    }


    [MessagePackObject]
    public class GameplayBuffCollection : BaseSmallIdObjectCollection<GameplayBuffStatus>
    {
        [Key(0)] public GameplayBuffStatus[] Data { get => _data; set => _data = value; }
        protected override GameplayBuffStatus InternalAdd(GameplayBuffStatus first, GameplayBuffStatus second)
        {
            throw new NotImplementedException("Cannot add two GameplayBuffStatuses together");
        }

        protected override bool IsDefault(GameplayBuffStatus t)
        {
            return t == default(GameplayBuffStatus);
        }
    }

    [MessagePackObject]
    public class GameplayBuffStatus
    {
        [Key(0)] public DateTime EndTime { get; set; }
    }




    public class GameplayStatDataLoader : UnitDataLoader<AttributeData> { }


    [MessagePackObject]
    public class GameplayStatDto : NoChildPlayerDataDto<AttributeData>
    {
        [Key(0)] public override AttributeData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class GameplayStatDataMapper : NoChildUnitDataMapper<AttributeData, GameplayStatDto> { }
}


