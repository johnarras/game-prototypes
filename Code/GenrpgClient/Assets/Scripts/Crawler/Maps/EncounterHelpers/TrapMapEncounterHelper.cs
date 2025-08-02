using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public class TrapMapEncounterHelper : BaseClientMapEncounterHelper
    {
        public override long Key => MapEncounters.Trap;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            LoadPropAtCell(mapRoot, cell, "Trap", x, z, null, token);

            await Task.CompletedTask;
        }

        protected override void AfterDownloadProp(GameObject prop, CrawlerObjectLoadData args)
        {
        }
    }
}
