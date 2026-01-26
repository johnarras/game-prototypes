using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public class TreasureMapEncounterHelper : BaseClientMapEncounterHelper
    {
        protected ILootGenService _lootGenService = null;

        public override long HelperKey => MapEncounters.Treasure;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            LoadPropAtCell(mapRoot, cell, "Chest", x, z, null, token);

            await Task.CompletedTask;
        }

        public override async Awaitable OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            LootGenData lootGenData = await _lootGenService.CreateLootGenData(party,
                MathUtil.FloatRange(2.0f, 4.0f, _rand), MathUtil.FloatRange(2.0f, 4.0f, _rand), MathUtil.FloatRange(2.0f, 4.0f, _rand), "You Found a Great Treasure!", ECrawlerStates.ExploreWorld, null);

            mapStatus.OneTimeEncounters.Add(new PointXZ() { X = party.CurrPos.X, Z = party.CurrPos.Z });
            _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
            _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
            moveStatus.MoveIsComplete = true;
        }

        protected override void AfterDownloadProp(GameObject prop, CrawlerObjectLoadData args)
        {
        }
    }
}


