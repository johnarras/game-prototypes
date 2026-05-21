using OxDb.RequestServer.Core;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Input.PlayerData;
using OxDb.SharedGame.SpellCrafting.Services;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Spells;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
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
                    newSpell.Id = HashUtils.NewGuid();
                    newSpell.OwnerId = ch.Id;
                    spellData.Add(newSpell);
                }

                ActionInputData adata = ch.Get<ActionInputData>();

                ActionInput ai = adata.GetData().FirstOrDefault(x => x.SpellId == i);
                if (ai == null)
                {
                    ActionInput newInput = adata.SetInput(i, i);
                    if (newInput != null)
                    {
                        await _repoService.Save(newInput);
                    }
                }
                else
                {
                    ai.Index = i;
                    await _repoService.Save(ai);
                }
            }

            foreach (Spell spell in spellData.GetData())
            {
                _spellCraftingService.CreateNewSpellData(ch, spell);
            }


            await Task.CompletedTask;
        }
    }
}


