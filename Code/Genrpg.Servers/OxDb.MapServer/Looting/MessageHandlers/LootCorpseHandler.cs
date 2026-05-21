using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.ServerGame.Achievements;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Loot.Messages;

using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.MapServer.Looting.MessageHandlers
{
    public class LootCorpseHandler : BaseCharacterServerMapMessageHandler<LootCorpse>
    {

        private IAchievementService _achievementService = null;

        protected override async Task InnerProcess(IRandomContainer rand, Character ch, LootCorpse message)
        {
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                ch.SendError("That can't be looted");
                return;
            }

            if (!UnitUtils.AttackerInfoMatchesObject(unit.GetFirstAttacker(), ch))
            {
                ch.SendError("You can't loot that!");
                return;
            }


            List<RewardList> loot = new List<RewardList>();
            lock (unit.OnActionLock)
            {
                if (unit.Loot == null || unit.Loot.Count < 1)
                {
                    ch.SendError("That has no loot");
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


