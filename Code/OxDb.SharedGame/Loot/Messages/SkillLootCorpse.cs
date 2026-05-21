using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Loot.Messages
{
    [MessagePackObject]
    public sealed class SkillLootCorpse : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


