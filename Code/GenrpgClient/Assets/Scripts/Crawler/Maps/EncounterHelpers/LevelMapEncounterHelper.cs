using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public class LevelMapEncounterHelper : BaseClientMapEncounterHelper
    {
        public override long HelperKey => MapEncounters.LevelMap;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {


            LoadPropAtCell(mapRoot, cell, "LevelMap", x, z, null, token);


            await Task.CompletedTask;
        }

        public override async Awaitable OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (!party.CompletedMaps.HasBit(party.CurrPos.MapId))
            {
                _crawlerService.ChangeState(ECrawlerStates.LevelMap, token);
                moveStatus.MoveIsComplete = true;
            }
            await Task.CompletedTask;
        }

        protected override void AfterDownloadProp(GameObject prop, CrawlerObjectLoadData args)
        {
        }
    }
}


