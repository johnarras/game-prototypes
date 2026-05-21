using OxDb.MapServer.Combat.Messages;
using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Maps;
using OxDb.MapServer.Spawns.Services;
using OxDb.ServerGame.Achievements;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Achievements.Constants;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.RpgLevels.Settings;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Units.Services
{
    public class ServerUnitService : IServerUnitService
    {
        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
        private IAchievementService _achievementService = null;
        private IRewardService _rewardService = null;

        public void CheckForDeath(IRandom rand, ActiveSpellEffect eff, Unit targ)
        {
            if (targ.HasFlag(UnitFlags.IsDead))
            {
                return;
            }

            targ.AddFlag(UnitFlags.IsDead);

            UnitType utype = _gameData.Get<UnitTypeSettings>(targ).Get(targ.EntityId);

            TribeType ttype = _gameData.Get<TribeSettings>(targ).Get(utype.TribeTypeId);

            AttackerInfo firstAttacker = targ.GetFirstAttacker();

            if (firstAttacker == null)
            {
                targ.AddAttacker(eff.CasterId, eff.CasterGroupId);
                firstAttacker = targ.GetFirstAttacker();
            }

            Died died = new Died()
            {
                UnitId = targ.Id,
                FirstAttacker = firstAttacker,
            };

            targ.Loot = new List<RewardList>();
            targ.SkillLoot = new List<RewardList>();

            RollLootArgs rollLootArgs = new RollLootArgs()
            {
                Level = targ.Level,
                Depth = 0,
                QualityTypeId = targ.QualityTypeId,
                Times = 1,
            };

            if (firstAttacker != null && _objectManager.GetChar(firstAttacker.AttackerId, out Character ch))
            {


                targ.SkillLoot = new List<RewardList>();

                targ.Loot = _spawnService.Roll(rand, _gameData.Get<SpawnSettings>(targ).MonsterLootSpawnTableId, RewardSources.Kill, rollLootArgs);
                RpgLevel levelData = _gameData.Get<RpgLevelSettings>(targ).Get(targ.Level);

                if (levelData != null)
                {
                    if (targ.Loot.Count < 1)
                    {
                        targ.Loot.Add(_rewardService.CreateRewardList(RewardSources.Kill, new List<Reward>(), targ.EntityId));
                    }
                    targ.Loot[0].Rewards.Add(new Reward()
                    {
                        EntityTypeId = EntityTypes.CharCurrency,
                        EntityId = CharCurrencyTypes.Money,
                        Quantity = RandUtils.LongRange(levelData.KillMoney / 2, levelData.KillMoney * 3 / 2, rand),
                    });
                }

                targ.Loot = targ.Loot.Where(x => x.Rewards.Count > 0).ToList();

                if (utype.LootItems != null)
                {
                    targ.Loot.AddRange(_spawnService.Roll(rand, utype.LootItems, RewardSources.Kill, rollLootArgs));
                }
                // Quest loot? need list of quests from caster?

                if (utype.InteractLootItems != null)
                {
                    targ.SkillLoot = _spawnService.Roll(rand, utype.InteractLootItems, RewardSources.SkillLoot, rollLootArgs);
                }

                if (ttype != null)
                {
                    targ.Loot.AddRange(_spawnService.Roll(rand, ttype.LootItems, RewardSources.Kill, rollLootArgs));
                    targ.SkillLoot.AddRange(_spawnService.Roll(rand, ttype.InteractLootItems, RewardSources.SkillLoot, rollLootArgs));
                }

                targ.SkillLoot = targ.SkillLoot.Where(x => x.Rewards.Count > 0).ToList();

                foreach (AttackerInfo info in targ.GetAttackers())
                {
                    if (_objectManager.GetChar(info.AttackerId, out Character ch2))
                    {
                        _achievementService.UpdateAchievement(ch2, AchievementConstants.KillMonsterStartId + utype.IdKey, 1);
                    }
                }
            }

            died.Loot = targ.Loot;
            died.SkillLoot = targ.SkillLoot;

            _messageService.SendMessageNear(targ, died, MessageConstants.DefaultGridDistance * 2);

            Killed killed = new Killed()
            {
                UnitTypeId = targ.EntityTypeId,
                FactionTypeId = targ.FactionTypeId,
                Level = targ.Level,
                ObjId = targ.Id,
                ZoneId = targ.ZoneId,
                UnitId = targ.Id,

            };

            if (_objectManager.GetUnit(eff.CasterId, out Unit killerUnit))
            {
                _messageService.SendMessage(killerUnit, killed);
            }

            _objectManager.RemoveObject(rand, targ.Id, UnitConstants.CorpseDespawnSeconds);

        }

        public bool IsOkUnit(Unit unit, bool playersOk)
        {
            if (unit == null)
            {
                return false;
            }

            if (!playersOk && unit.IsPlayer())
            {
                return false;
            }

            if (unit.IsDeleted())
            {
                return false;
            }

            if (unit.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }
            return true;
        }
    }
}


