using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public interface IRiddleTypeHelper : ISetupDictionaryItem<long>
    {
        Task AddRiddle(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand);
        bool ShouldDrawProp(PartyData party, int x, int z);
        void SetPropPosition(object prop, object data, CancellationToken token);
    }
}
