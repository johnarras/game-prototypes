using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using MessagePack;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class CraftSpell : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public Spell CraftedSpell { get; set; }
    }
}


