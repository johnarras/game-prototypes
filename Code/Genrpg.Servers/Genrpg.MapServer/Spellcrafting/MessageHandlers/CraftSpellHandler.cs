using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Utils;
using System.Linq;

namespace Genrpg.MapServer.Spellcrafting.MessageHandlers
{
    public class CraftSpellHandler : BaseCharacterServerMapMessageHandler<CraftSpell>
    {
        private ISharedSpellCraftService _spellCraftService = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, CraftSpell message)
        {
            Spell startSpell = message.CraftedSpell;

            if (startSpell == null)
            {
                return;
            }

            startSpell.OwnerId = ch.Id;
            if (string.IsNullOrEmpty(startSpell.Id))
            {
                startSpell.Id = HashUtils.NewGuid();
            }

            Spell newSpell = _spellCraftService.CreateNewSpellData(ch, startSpell);

            if (newSpell == null)
            {
                ch.AddMessage(new ErrorMessage("Failed to craft spell!"));
            }
            else
            {
                _repoService.QueueSave(newSpell);
            }

            SpellData spellData = ch.Get<SpellData>();

            long maxId = 0;

            if (spellData.GetData().Count > 0)
            {
                maxId = spellData.GetData().Max(x => x.IdKey);
            }

            if (startSpell.IdKey < 1)
            {
                startSpell.IdKey = ++maxId;
            }

            spellData.Add(startSpell);
            _repoService.QueueSave(startSpell);
            ch.AddMessage(new OnCraftSpell() { CraftedSpell = startSpell });
        }
    }
}


