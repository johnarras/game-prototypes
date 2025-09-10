using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Items.Entities
{
    // MessagePackIgnore
    public class MemberItemSpell
    {
        public PartyMember Member { get; set; }
        public Item UsableItem { get; set; }
        public CrawlerSpell Spell { get; set; }
        public long ChargesLeft { get; set; }

        public string GetDescription()
        {
            return Member.Name + ": Cast " + Spell.Name + " With " + UsableItem.Name + " (" + ChargesLeft + ")";
        }
    }
}
