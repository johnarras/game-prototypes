using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Input.PlayerData;
using OxDb.SharedGame.SpellCrafting.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spellcrafting.MessageHandlers
{
    public class SetActionBarItemHandler : BaseCharacterServerMapMessageHandler<SetActionBarItem>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, SetActionBarItem message)
        {
            ActionInputData actionData = ch.Get<ActionInputData>();

            ActionInput newInput = actionData.SetInput(message.Index, message.SpellId);
            if (newInput != null)
            {
                _repoService.Save(newInput);
            }

            ch.AddMessage(new OnSetActionBarItem() { Index = message.Index, SpellId = message.SpellId });
            await Task.CompletedTask;

        }
    }
}


