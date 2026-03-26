using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.RpgLevels.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Quests.Services
{
    public interface ISharedQuestService : IInjectable
    {
        int GetQuestState(IRandom rand, Character ch, QuestType qtype);
        bool IsQuestSoonVisible(IRandom rand, Character ch, QuestType qtype);
        List<Reward> GetRewards(IRandom rand, Character ch, QuestType qtype, bool createRewards = false);

    }


    public class SharedQuestService : ISharedQuestService
    {

        private IGameData _gameData = null;

        public int GetQuestState(IRandom rand, Character ch, QuestType qtype)
        {
            if (qtype == null)
            {
                return QuestState.NotAvailable;
            }

            QuestData questList = ch.Get<QuestData>();

            QuestStatus currQuest = questList.GetStatus(qtype);

            if (currQuest == null)
            {
                if (ch.Level >= qtype.MinLevel)
                {
                    return QuestState.Available;
                }
                else if (ch.Level >= qtype.MinLevel - QuestConstants.QuestAlmostVisibleLevels)
                {
                    return QuestState.AlmostAvailable;
                }
                else
                {
                    return QuestState.NotAvailable;
                }
            }
            else
            {
                if (currQuest.IsComplete())
                {
                    return QuestState.Complete;
                }

                return QuestState.Active;
            }



        }

        public virtual bool IsQuestSoonVisible(IRandom rand, Character ch, QuestType qtype)
        {

            if (ch.Level < qtype.MinLevel - QuestConstants.QuestAlmostVisibleLevels)
            {
                return false;
            }

            return true;
        }


        public List<Reward> GetRewards(IRandom rand, Character ch, QuestType qtype, bool createRewards = false)
        {
            List<Reward> rewards = new List<Reward>();

            if (qtype == null)
            {
                return rewards;
            }

            RpgLevel level = _gameData.Get<RpgLevelSettings>(ch).Get(qtype.MinLevel);

            if (level == null)
            {
                return rewards;
            }

            rewards.Add(new Reward()
            {
                EntityTypeId = EntityTypes.CharCurrency,
                EntityId = CharCurrencyTypes.Exp,
                Quantity = qtype.CurrencyScale * level.QuestExp
            });
            rewards.Add(new Reward()
            {
                EntityTypeId = EntityTypes.CharCurrency,
                EntityId = CharCurrencyTypes.Money,
                Quantity = (long)(qtype.CurrencyScale * level.KillMoney * QuestConstants.QuestKillMoneyMultiplier),
            });

            return rewards;
        }
    }
}


