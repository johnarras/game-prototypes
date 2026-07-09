using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.GroundObjects.Settings;
using OxDb.SharedGame.Interactions.Messages;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Loot.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Services;
using OxDb.SharedGame.Spawns.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.MapServer.InteractObject.MessageHandlers
{
    public class CompleteInteractHandler : BaseCharacterServerMapMessageHandler<CompleteInteract>
    {
        private ISpawnService _spawnService = null;

        protected override async ValueTask InnerProcess(Character ch, CompleteInteract message)
        {
            await Task.CompletedTask;
            string errorMessage = "";
            if (ch.ActionMessage == null)
            {
                errorMessage = "You aren't casting";
            }

            if (string.IsNullOrEmpty(errorMessage) && ch.ActionMessage != message)
            {
                errorMessage = "You aren't casting this";
            }

            MapObject target = null;
            if (string.IsNullOrEmpty(errorMessage) &&
                !_objectManager.GetObject(message.TargetId, out target))
            {
                errorMessage = "Target doesn't exist";
            }
            CompleteInteract targetAction = null;
            if (string.IsNullOrEmpty(errorMessage))
            {
                targetAction = target.OnActionMessage as CompleteInteract;
                if (targetAction == null || targetAction != message)
                {
                    errorMessage = "Target was busy";
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ch.SendError(errorMessage);
                message.SetCancelled(true);
                if (ch.ActionMessage != null)
                {
                    ch.ActionMessage.SetCancelled(true);
                }
                if (targetAction != null)
                {
                    targetAction.SetCancelled(true);
                }
                return;
            }

            if (!message.IsSkillLoot)
            {
                GroundObjType gtype = _gameData.Get<GroundObjTypeSettings>(ch).Get(message.GroundObjTypeId);

                if (gtype != null && gtype.SpawnTableId > 0)
                {
                    List<SpawnItem> lootItems = new List<SpawnItem>();
                    lootItems.Add(new SpawnItem()
                    {
                        EntityTypeId = EntityTypes.Spawn,
                        EntityId = gtype.SpawnTableId,
                        MinQuantity = gtype.MinRolls,
                        MaxQuantity = gtype.MaxRolls,
                    });

                    RollLootArgs rollLootArgs = new RollLootArgs()
                    {
                        Level = message.Level,
                        QualityTypeId = QualityTypes.Common,
                        Times = 1,
                    };
                    List<RewardList> rewards = await _spawnService.Roll(ch, lootItems, RewardSources.SkillLoot, rollLootArgs);

                    if (rewards.Count > 0)
                    {
                        await _rewardService.GiveRewards(ch, rewards, null);

                        SendRewards sendLoot = new SendRewards()
                        {
                            ShowPopup = true,
                            Rewards = rewards,
                        };
                        ch.AddMessage(sendLoot);
                    }
                }
            }
            else
            {
                if (_objectManager.GetUnit(message.TargetId, out Unit unit))
                {
                    if (unit.SkillLoot != null && unit.SkillLoot.Count > 0)
                    {
                        await _rewardService.GiveRewards(ch, unit.SkillLoot, null);

                        SendRewards sendLoot = new SendRewards()
                        {
                            ShowPopup = true,
                            Rewards = unit.SkillLoot,
                        };
                        ch.AddMessage(sendLoot);
                        unit.SkillLoot = null;
                    }
                }
            }
            message.SetCancelled(true);
            ch.ActionMessage = null;
            target.OnActionMessage = null;
            _objectManager.RemoveObject(ch.Rand, target.Id, 0);
        }
    }
}


