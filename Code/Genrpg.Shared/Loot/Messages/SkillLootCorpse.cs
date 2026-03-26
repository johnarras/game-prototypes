using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Loot.Messages
{
    [MessagePackObject]
    public sealed class SkillLootCorpse : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


