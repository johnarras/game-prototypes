using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class SetActionBarItemHandler : BaseCharacterServerMapMessageHandler<SetActionBarItem>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, SetActionBarItem message)
        {
            ActionInputData actionData = ch.Get<ActionInputData>();

            ActionInput newInput = actionData.SetInput(message.Index, message.SpellId);
            if (newInput != null)
            {
                _repoService.Save(newInput);
            }

            ch.AddMessage(new OnSetActionBarItem() { Index = message.Index, SpellId = message.SpellId });

        }
    }
}


