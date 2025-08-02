using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.UserCoins.Constants;

namespace Genrpg.Shared.UserCoins.Settings
{
    [MessagePackObject]
    public class UserCoinSettings : ParentConstantListSettings<UserCoinType,UserCoinTypes>
    {
        [Key(0)] public override string Id { get; set; }


        public string GetName(long userCoinTypeId)
        {
            return Get(userCoinTypeId)?.Name ?? "Unknown";
        }
    }
    [MessagePackObject]
    public class UserCoinType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }
        [Key(9)] public int BaseStorage { get; set; }

    }
    public class UserCoinSettingsDto : ParentSettingsDto<UserCoinSettings, UserCoinType> { }
    public class UnitCoinSettingsLoader : ParentSettingsLoader<UserCoinSettings, UserCoinType> { }

    public class UserCoinSettingsMapper : ParentSettingsMapper<UserCoinSettings, UserCoinType, UserCoinSettingsDto> { }


    public class UserCoinHelper : BaseEntityHelper<UserCoinSettings, UserCoinType>
    {
        public override long Key => EntityTypes.UserCoin;
    }
    public class UserCoinRewardMultHelper : BaseEntityHelper<UserCoinSettings, UserCoinType>
    {
        public override long Key => EntityTypes.UserCoinRewardMult;
    }
    public class UserCoinMaxStorageHelper : BaseEntityHelper<UserCoinSettings, UserCoinType>
    {
        public override long Key => EntityTypes.UserCoinMaxStorage;
    }
}
