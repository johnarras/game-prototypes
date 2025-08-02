using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public abstract class BaseRiddleTypeHelper : IRiddleTypeHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;

        public abstract long Key { get; }

        public virtual void SetPropPosition(object prop, object data, CancellationToken token)
        {
        }

        public virtual bool ShouldDrawProp(PartyData party, int x, int z)
        {
            return true;
        }

        public async Task AddRiddle(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            if (!DataIsOk(lookup, lockedFloor, prevFloor, openPoints, rand))
            {
                return;
            }

            prevFloor.RiddleHints = new MapRiddleHints() { RiddleTypeId = Key };
            lockedFloor.EntranceRiddle = new MapEntranceRiddle() { RiddleTypeId = Key };

            if (!await AddRiddleInternal(lookup, lockedFloor, prevFloor, openPoints, rand))
            {
                prevFloor.RiddleHints = null;
                lockedFloor.EntranceRiddle = null;
            }

            await Task.CompletedTask;
        }

        protected bool DataIsOk(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            return lookup != null && lockedFloor != null && prevFloor != null && openPoints.Count > 0 && rand != null &&
                prevFloor.RiddleHints == null && lockedFloor.EntranceRiddle == null;
        }

        protected abstract Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand);

    }
}
