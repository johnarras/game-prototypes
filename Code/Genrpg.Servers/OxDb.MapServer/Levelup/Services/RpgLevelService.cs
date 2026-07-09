using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.Rewards.Constants;

using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.RpgLevels.Messages;
using OxDb.SharedGame.RpgLevels.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace OxDb.MapServer.Levelup.Services
{
    public interface IRpgLevelService : IInjectable
    {
        Task UpdateLevel(Character ch);
        void SetupLevels(GameData data);
        Task<bool> GiveLevelRewards(Character ch, RpgLevel lev);

    }

    public class RpgLevelService : IRpgLevelService
    {
        private IRewardService _rewardService = null;
        private IStatService _statService = null;
        private IMapMessageService _messageService = null;
        private IGameData _gameData = null;

        public async Task UpdateLevel(Character ch)
        {
            CharCurrencyData currencies = ch.Get<CharCurrencyData>();

            long startLevel = ch.Level;
            long maxLevel = _gameData.Get<RpgLevelSettings>(ch).MaxLevel;
            long startExp = currencies.Data[CharCurrencyTypes.Exp];
            long currExp = startExp;
            long endLevel = startLevel;
            for (endLevel = startLevel; endLevel < maxLevel; endLevel++)
            {
                RpgLevel ldata = _gameData.Get<RpgLevelSettings>(ch).Get(endLevel);
                if (ldata == null)
                {
                    break;
                }

                if (ldata.CurrExp > currExp)
                {
                    break;
                }

                currExp -= ldata.CurrExp;
                currExp = 0;
                ch.Level = endLevel + 1;
                NewRpgLevel levelMessage = new NewRpgLevel()
                {
                    Level = ch.Level,
                    UnitId = ch.Id,
                };
                _messageService.SendMessageNear(ch, levelMessage);
                await GiveLevelRewards(ch, ldata);
            }

            if (endLevel > startLevel)
            {
                long oldExp = currencies.Data[CharCurrencyTypes.Exp];
                await _rewardService.GiveReward(ch, EntityTypes.CharCurrency, CharCurrencyTypes.Exp, currExp - oldExp, RewardSources.LevelUp, null, 0, null);
                _statService.CalcStats(ch, true);
            }
        }

        public virtual async Task<bool> GiveLevelRewards(Character ch, RpgLevel lev)
        {

            if (lev == null)
            {
                return false;
            }
            /// Don't give rewards more than once.
            if (lev.IdKey <= ch.Level)
            {
                return true;
            }

            ch.Level = (int)lev.IdKey;

            if (lev.RewardList != null)
            {
                await _rewardService.GiveRewards(ch, _rewardService.CreateListFromList(RewardSources.Kill, lev.IdKey, lev.RewardList), null);
            }

            ch.AbilityPoints += lev.AbilityPoints;

            return true;
        }


        public void SetupLevels(GameData data)
        {
            return;
        }

        // Scale with damage player does.
        protected float StatPercentPoints(long lev)
        {
            return (float)Math.Round(1 + (0.1f + PlayerDamage(lev) * 0.027f), 1);
        }

        // Health functions

        protected long MonsterHealth(long lev)
        {
            return (long)(1.00f * (30 +
                5 * Math.Pow(lev, 1.4f) +
                0.19f * Math.Pow(lev, 2.6f) +
                1.8 * Math.Pow(2, lev / 8f)));
        }
        protected float MobDieTime(long lev)
        {
            return (float)(3.8f + lev / 17.0f);
        }

        protected long PlayerHealth(long lev)
        {
            return (long)(MonsterHealth(lev) * (1.5f + lev / 130.0f));
        }

        // Average damage functions

        protected long MonsterDamage(long lev)
        {
            return (long)(0.7f * (MonsterHealth(lev) / MobDieTime(lev)));
        }

        protected long PlayerDamage(long lev)
        {
            return (long)(MonsterHealth(lev) / MobDieTime(lev));

        }

        // Advancement functions


        protected float MobCount(long lev)
        {
            return 6 + 1.3f * lev;
        }

        protected float QuestCount(long lev)
        {
            return 1.3f + lev / 6.5f;
        }

        protected long MobExp(long lev)
        {
            return (long)(50 + 20 * lev + 2 * Math.Pow(lev, 1.5f));
        }

        protected long QuestExp(long lev)
        {
            return 100 + 75 * lev;
        }

        protected int SkillPoints(long lev)
        {
            int val = 3;
            if (lev % 5 == 1)
            {
                val += 2;
            }
            return val;
        }

        protected const int MinStatPercentPoints = 1;


    }
}


