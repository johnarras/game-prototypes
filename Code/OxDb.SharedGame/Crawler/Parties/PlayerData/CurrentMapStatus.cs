using OxDb.SharedCore.Utils.Data;

namespace OxDb.SharedGame.Crawler.Parties.PlayerData
{
    public class CurrentMapStatus
    {
        public SmallIndexBitList Visited { get; set; } = new SmallIndexBitList();
        public SmallIndexBitList Cleansed { get; set; } = new SmallIndexBitList();


        public void Clear()
        {
            Visited.Clear();
            Cleansed.Clear();
        }
    }
}


