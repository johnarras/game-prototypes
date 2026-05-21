using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.SpellCrafting.Messages;
using OxDb.SharedGame.SpellCrafting.Services;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spellcrafting.MessageHandlers
{
    public class CraftSpellHandler : BaseCharacterServerMapMessageHandler<CraftSpell>
    {
        private ISharedSpellCraftService _spellCraftService = null;

        protected override async Task InnerProcess(IRandomContainer rand, Character ch, CraftSpell message)
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
                ch.SendError("Failed to craft spell!");
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
            await Task.CompletedTask;
        }
    }
}


