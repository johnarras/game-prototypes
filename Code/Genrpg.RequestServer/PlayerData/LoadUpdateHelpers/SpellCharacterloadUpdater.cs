using Genrpg.RequestServer.Core;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Serialization.Interfaces;
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

        protected IFullRepositoryService _repoService = null;
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
                    ActionInput newInput = adata.SetInput(i, i);
                    if (newInput != null)
                    {
                        _repoService.QueueSave(newInput);
                    }
                }
                else
                {
                    ai.Index = i;
                    _repoService.QueueSave(ai);
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


