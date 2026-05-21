using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
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
            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
            };

            _mapService.LoadProp(loadData, "Chest", token);

            await Task.CompletedTask;
        }

        public override async Awaitable OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            LootGenData lootGenData = await _lootGenService.CreateLootGenData(party,
                RandUtils.FloatRange(2.0f, 4.0f, _rand.Rand), RandUtils.FloatRange(2.0f, 4.0f, _rand.Rand), RandUtils.FloatRange(2.0f, 4.0f, _rand.Rand), "You Found a Great Treasure!", ECrawlerStates.ExploreWorld, null);

            mapStatus.OneTimeEncounters.Add(new PointXZ() { X = party.CurrPos.X, Z = party.CurrPos.Z });
            _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
            _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
            moveStatus.MoveIsComplete = true;
        }
    }
}


