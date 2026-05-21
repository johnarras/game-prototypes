using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crawler.Items.Entities
{
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


