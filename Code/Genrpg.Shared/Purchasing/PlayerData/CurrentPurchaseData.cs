using MessagePack;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;

namespace Genrpg.Shared.Purchasing.PlayerData
{


    [MessagePackObject]
    public class CurrentPurchaseData : NoChildPlayerData, IUserData, IServerOnlyData
    {

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public PlayerStoreOffer StoreOffer { get; set; }

        [Key(2)] public PlayerStoreOfferItem StoreItem { get; set; }

        [Key(3)] public ECurrentPurchaseStates State { get; set; }
    }

    public class CurrentPurchaseDto : NoChildPlayerDataDto<CurrentPurchaseData> { }


    public class CurrentPurchaseDataMapper : NoChildUnitDataMapper<CurrentPurchaseData, CurrentPurchaseDto> { }


    public class CurrentPurchaseLoader : UnitDataLoader<CurrentPurchaseData> { }
}
