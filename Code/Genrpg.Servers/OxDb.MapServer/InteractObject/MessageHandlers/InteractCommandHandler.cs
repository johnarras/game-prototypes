using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Crafting.Constants;
using OxDb.SharedGame.Crafting.PlayerData.Crafting;
using OxDb.SharedGame.Crafting.Settings.Crafters;
using OxDb.SharedGame.GroundObjects.Settings;
using OxDb.SharedGame.Interactions.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.InteractObject.MessageHandlers
{
    public class InteractCommandHandler : BaseMapObjectServerMapMessageHandler<InteractCommand>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, InteractCommand message)
        {
            await Task.CompletedTask;
            if (!_objectManager.GetObject(message.TargetId, out MapObject target))
            {
                obj.SendError("Object does not exist!");
                return;
            }

            if (obj.ActionMessage != null)
            {
                obj.SendError("You are already busy");
                return;
            }

            string actionName = "Gathering";
            string animName = "Gather";

            long crafterId = 0;

            float gatherSeconds = 0;

            long level = target.Level;
            int skillPoints = 0;
            long groundObjTypeId = 0;

            if (target.EntityTypeId == EntityTypes.GroundObject)
            {
                GroundObjType gtype = _gameData.Get<GroundObjTypeSettings>(obj).Get(target.EntityId);

                if (gtype == null)
                {
                    obj.SendError("Invalid object type");
                    return;
                }
                groundObjTypeId = gtype.IdKey;
                crafterId = gtype.CrafterTypeId;

                CrafterType ctype = _gameData.Get<CraftingSettings>(obj).Get(crafterId);
                if (ctype != null)
                {
                    actionName = ctype.GatherActionName;
                    animName = ctype.GatherAnimation;
                    if (obj is Character ch)
                    {
                        CraftingData cdata = ch.Get<CraftingData>();
                        skillPoints = cdata.Get(crafterId).GetSkillPoints(CraftingConstants.GatheringSkill);
                    }
                    gatherSeconds = ctype.GatherSeconds;
                }
                else
                {
                    crafterId = 0;
                }
            }

            OnStartCast startCast = obj.GetCachedMessage<OnStartCast>(true);
            startCast.CasterId = obj.Id;
            startCast.CastSeconds = gatherSeconds;
            startCast.CastingName = actionName;
            startCast.AnimName = animName;


            CompleteInteract completeInteract = new CompleteInteract();
            completeInteract.CasterId = obj.Id;
            completeInteract.TargetId = target.Id;
            completeInteract.CrafterTypeId = crafterId;
            completeInteract.Level = level;
            completeInteract.SkillPoints = skillPoints;
            completeInteract.GroundObjTypeId = groundObjTypeId;
            completeInteract.IsSkillLoot = message.IsSkillLoot;
            lock (target.OnActionLock)
            {
                if (target.OnActionMessage != null && !target.OnActionMessage.IsCancelled())
                {
                    obj.SendError("Object is in use");
                    return;
                }
                else
                {
                    target.OnActionMessage = completeInteract;
                    obj.ActionMessage = completeInteract;
                }
            }


            _messageService.SendMessageNear(obj, startCast);

            _messageService.SendMessage(obj, completeInteract, gatherSeconds);
        }
    }
}


