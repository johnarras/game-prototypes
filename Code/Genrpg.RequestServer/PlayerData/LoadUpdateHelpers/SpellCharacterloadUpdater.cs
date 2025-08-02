using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class SpellCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private ISharedSpellCraftService _spellCraftingService = null;
        private IGameData _gameData = null;
        private ITextSerializer _serializer = null;

        protected IRepositoryService _repoService = null;
        public override int Order => 2;

        public override async Task Update(WebContext context, Character ch)
        {
            SpellData spellData = ch.Get<SpellData>();
            for (int i = 1; i <= 3; i++)
            {
                Spell mySpell = spellData.Get(i);
                if (mySpell == null)
                {
                    Spell newSpell = _serializer.ConvertType<SpellType, Spell>(_gameData.Get<SpellTypeSettings>(ch).Get(i));
                    newSpell.Id = HashUtils.NewUUId();
                    newSpell.OwnerId = ch.Id;
                    spellData.Add(newSpell);
                }

                ActionInputData adata = ch.Get<ActionInputData>();

                ActionInput ai = adata.GetData().FirstOrDefault(x => x.SpellId == i);
                if (ai == null)
                {
                    adata.SetInput(i, i, _repoService);
                }
                else
                {
                    ai.Index = i;
                    _repoService.QueueSave(ai);
                }
            }

            foreach (Spell spell in spellData.GetData())
            {
                _spellCraftingService.ValidateSpellData(ch, spell);
            }


            await Task.CompletedTask;
        }
    }
}
