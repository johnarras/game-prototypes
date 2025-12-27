using MessagePack;

using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.Entities
{
    public class ForcedNextState
    {
        public ECrawlerStates NextState { get; set; }
        public MapCellDetail Detail { get; set; }
    }
}


