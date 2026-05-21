using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Crafting.Constants;
using OxDb.SharedGame.Crafting.PlayerData.Crafting;
using OxDb.SharedGame.Crafting.Settings.Crafters;
using OxDb.SharedGame.Interactions.Messages;
using OxDb.SharedGame.Loot.Messages;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Threading.Tasks;

namespace OxDb.MapServer.Looting.MessageHandlers
{
    public class SkillLootCorpseHandler : BaseUnitServerMapMessageHandler<SkillLootCorpse>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Unit looter, SkillLootCorpse message)
        {

            await Task.CompletedTask;
            if (looter.ActionMessage != null)
            {
                looter.SendError("You are already busy");
                return;
            }

            if (!_objectManager.GetUnit(message.UnitId, out Unit target))
            {
                looter.SendError("Target does not exist");
                return;
            }

            if (target.SkillLoot == null || target.SkillLoot.Count < 1)
            {
                looter.SendError("Target has no loot");
                return;
            }

            UnitType utype = _gameData.Get<UnitTypeSettings>(target).Get(target.EntityId);
            if (utype == null)
            {
                looter.SendError("Not a valid target");
                return;
            }

            TribeType tribeType = _gameData.Get<TribeSettings>(target).Get(utype.TribeTypeId);

            if (tribeType == null)
            {
                looter.SendError("Not a valid type");
                return;
            }
            CrafterType crafterType = _gameData.Get<CraftingSettings>(looter).Get(tribeType.LootCrafterTypeId);

            if (crafterType == null)
            {
                looter.SendError("This unit has no resources");
                return;
            }

            string actionName = crafterType.GatherActionName;
            string animName = crafterType.GatherAnimation;
            float gatherSeconds = crafterType.GatherSeconds;
            long level = looter.Level;
            int skillPoints = 0;

            if (looter is Character ch)
            {
                CraftingData cdata = ch.Get<CraftingData>();
                skillPoints = cdata.Get(crafterType.IdKey).GetSkillPoints(CraftingConstants.GatheringSkill);
            }

            OnStartCast startCast = new OnStartCast()
            {
                CasterId = looter.Id,
                CastSeconds = gatherSeconds,
                CastingName = actionName,
                AnimName = animName,
            };

            CompleteInteract completeInteract = new CompleteInteract()
            {
                CasterId = looter.Id,
                TargetId = target.Id,
                CrafterTypeId = crafterType.IdKey,
                Level = level,
                SkillPoints = skillPoints,
                GroundObjTypeId = 0,
                IsSkillLoot = true,
            };


            lock (looter.OnActionLock)
            {
                if (target.OnActionMessage != null && !target.OnActionMessage.IsCancelled())
                {
                    looter.SendError("Object is in use");
                    return;
                }
                else
                {
                    target.OnActionMessage = completeInteract;
                    looter.ActionMessage = completeInteract;
                }
            }

            _messageService.SendMessageNear(looter, startCast);

            _messageService.SendMessage(looter, completeInteract, gatherSeconds);

        }
    }
}


