using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Spells.PlayerData.Spells;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnCraftSpell : BaseInfrequenMapApiMessage
    {
        [Key(0)] public Spell CraftedSpell { get; set; }
    }
}


