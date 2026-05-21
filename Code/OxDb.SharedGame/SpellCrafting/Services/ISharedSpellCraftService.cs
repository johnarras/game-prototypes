using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.SpellCrafting.SpellModifierHelpers;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.PlayerData.Spells;

namespace OxDb.SharedGame.SpellCrafting.Services
{
    public interface ISharedSpellCraftService : IInjectable
    {
        Spell CreateNewSpellData(MapObject obj, ISpell spellType);
        ISpellModifierHelper GetSpellModifierHelper(long spellModifierId);
    }
}


