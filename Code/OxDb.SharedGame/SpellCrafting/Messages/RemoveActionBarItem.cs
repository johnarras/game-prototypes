using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class RemoveActionBarItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public int Index { get; set; }
    }
}


