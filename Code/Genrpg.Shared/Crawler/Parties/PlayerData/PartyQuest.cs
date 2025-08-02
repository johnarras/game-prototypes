using MessagePack;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class PartyQuest 
    {
        [Key(0)] public long CrawlerQuestId { get; set; }
        [Key(1)] public long CurrQuantity { get; set; }
    }
}
