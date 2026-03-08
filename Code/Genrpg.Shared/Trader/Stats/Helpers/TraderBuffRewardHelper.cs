using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using Genrpg.Shared.Trader.Stats.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trader.Stats.Helpers
{
    public class TraderBuffRewardHelper : IRewardHelper
    {
        private ITraderStatService _statService = null;
        public long HelperKey => EntityTypes.TraderBuff;

        public long GetQuantity(MapObject obj, long entityId)
        {
            CoreData coreData = obj.Get<CoreData>();
            CaravanData caravanData = obj.Get<CaravanData>();
            TraderStatData statData = obj.Get<TraderStatData>();

            return _statService.GetBuffSeconds(coreData, caravanData, statData, entityId);
        }

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            CoreData coreData = obj.Get<CoreData>();
            CaravanData caravanData = obj.Get<CaravanData>();
            TraderStatData statData = obj.Get<TraderStatData>();

            _statService.AddBuff(coreData, caravanData, statData, entityId, quantity);

            return true;
        }
    }
}
