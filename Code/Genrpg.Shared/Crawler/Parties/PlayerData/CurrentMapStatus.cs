using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
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


