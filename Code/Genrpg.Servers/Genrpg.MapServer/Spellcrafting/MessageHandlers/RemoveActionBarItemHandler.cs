using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class RemoveActionBarItemHandler : BaseCharacterServerMapMessageHandler<RemoveActionBarItem>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, RemoveActionBarItem message)
        {
            ActionInputData actionData = ch.Get<ActionInputData>();

            ActionInput input = actionData.GetInput(message.Index);

            if (input != null)
            {
                ActionInput newInput = actionData.SetInput(message.Index, 0);
                if (newInput != null)
                {
                    _repoService.Save(newInput);
                }
                ch.AddMessage(new OnRemoveActionBarItem() { Index = message.Index });
            }
        }
    }
}


