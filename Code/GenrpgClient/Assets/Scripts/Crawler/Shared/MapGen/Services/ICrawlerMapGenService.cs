using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.MapGen.Services
{
    public interface ICrawlerMapGenService : IInitializable
    {
        ICrawlerMapGenHelper GetGenHelper(long mapType);
        Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token);
    }
}
