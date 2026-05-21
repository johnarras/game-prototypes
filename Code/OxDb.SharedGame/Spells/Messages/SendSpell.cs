using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Stats.Entities;

namespace OxDb.SharedGame.Spells.Messages
{
    public sealed class SendSpell : BaseMapMessage
    {

        public string CasterId { get; set; }
        public string CasterGroupId { get; set; }
        public long CasterLevel { get; set; }
        public long CasterFactionId { get; set; }
        [IgnoreMember] public ReadOnlyStatGroup CasterStats { get; set; }
        public Spell Spell { get; set; }
        public ElementType ElementType { get; set; }

        public SendSpell()
        {
        }
    }
}


