using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Riddles.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Riddles.EntranceRiddleHelpers
{
    public interface IRiddleTypeHelper : ISetupDictionaryItem<long>
    {
        Task AddRiddle(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand);
        bool ShouldDrawProp(PartyData party, int x, int z);
        void SetPropPosition(object prop, object data, CancellationToken token);
    }
}


