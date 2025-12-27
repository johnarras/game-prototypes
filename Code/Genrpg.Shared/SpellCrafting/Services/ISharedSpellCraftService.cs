using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.PlayerData.Spells;

namespace Genrpg.Shared.SpellCrafting.Services
{
    public interface ISharedSpellCraftService : IInjectable
    {
        Spell CreateNewSpellData(MapObject obj, ISpell spellType);
        ISpellModifierHelper GetSpellModifierHelper(long spellModifierId);
    }
}


