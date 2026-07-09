using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Input.PlayerData;
using OxDb.SharedGame.SpellCrafting.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spellcrafting.MessageHandlers
{
    public class RemoveActionBarItemHandler : BaseCharacterServerMapMessageHandler<RemoveActionBarItem>
    {
        protected override async ValueTask InnerProcess(Character ch, RemoveActionBarItem message)
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
            await Task.CompletedTask;
        }
    }
}


