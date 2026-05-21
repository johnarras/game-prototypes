using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Input.PlayerData;
using OxDb.SharedGame.SpellCrafting.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spellcrafting.MessageHandlers
{
    public class DeleteSpellHandler : BaseCharacterServerMapMessageHandler<DeleteSpell>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, DeleteSpell message)
        {
            await Task.CompletedTask;
            SpellData spellData = ch.Get<SpellData>();

            List<Spell> deleteSpells = spellData.GetData().Where(x => x.IdKey == message.SpellId).ToList();

            if (deleteSpells.Count < 1)
            {
                ch.SendError("Missing spell");
            }

            spellData.SetData(spellData.GetData().Where(x => x.IdKey != message.SpellId).ToList());
            foreach (Spell spell in deleteSpells)
            {
                _repoService.QueueDelete(spell);
            }
            ActionInputData actionData = ch.Get<ActionInputData>();

            List<ActionInput> removeInputs = actionData.GetData().Where(x => x.SpellId == message.SpellId).ToList();

            ch.AddMessage(new OnDeleteSpell() { SpellId = message.SpellId });
            foreach (ActionInput removeInput in removeInputs)
            {
                ch.AddMessage(new OnRemoveActionBarItem() { Index = removeInput.Index });
                ActionInput input = actionData.SetInput(removeInput.Index, 0);
                if (input != null)
                {
                    _repoService.QueueSave(input);
                }

            }

        }
    }
}


