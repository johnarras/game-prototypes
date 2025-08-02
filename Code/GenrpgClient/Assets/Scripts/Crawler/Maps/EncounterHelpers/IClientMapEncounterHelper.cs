using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public interface IClientMapEncounterHelper : ISetupDictionaryItem<long>
    {
        Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token);
    }
}
