using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnRemoveActionBarItem : BaseInfrequenMapApiMessage
    {
        [Key(0)] public int Index { get; set; }
    }
}


