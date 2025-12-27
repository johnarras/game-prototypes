using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Entities;
using MessagePack;

namespace Genrpg.Shared.Spells.Messages
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


