using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class LootCorpseHandler : BaseCharacterServerMapMessageHandler<LootCorpse>
    {

        private IAchievementService _achievementService = null;

        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, LootCorpse message)
        {
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                pack.SendError(ch, "That can't be looted");
                return;
            }

            if (!UnitUtils.AttackerInfoMatchesObject(unit.GetFirstAttacker(), ch))
            {
                pack.SendError(ch, "You can't loot that!");
                return;
            }


            List<RewardList> loot = new List<RewardList>();
            lock (unit.OnActionLock)
            {
                if (unit.Loot == null || unit.Loot.Count < 1)
                {
                    pack.SendError(ch, "That has no loot");
                    return;
                }
                loot = unit.Loot;
                unit.Loot = null;
            }

            long moneyTotal = 0;
            long itemTotal = 0;
            foreach (RewardList rewardList in loot)
            {
                moneyTotal += rewardList.Rewards.Where(x => x.EntityTypeId == EntityTypes.CharCurrency && x.EntityId == CharCurrencyTypes.Money).Sum(x => x.Quantity);
                itemTotal += rewardList.Rewards.Where(x => x.EntityTypeId == EntityTypes.Item && x.ExtraData != null).Sum(x => x.Quantity);
            }

            _achievementService.UpdateAchievement(ch, AchievementTypes.MoneyLooted, moneyTotal);
            _achievementService.UpdateAchievement(ch, AchievementTypes.ItemsLooted, itemTotal);

            await _rewardService.GiveRewards(ch, loot, null);
            SendRewards sendLoot = new SendRewards()
            {
                ShowPopup = true,
                Rewards = loot,
            };
            ch.AddMessage(sendLoot);

            _messageService.SendMessageNear(unit, new ClearLoot() { UnitId = unit.Id });
        }
    }
}


