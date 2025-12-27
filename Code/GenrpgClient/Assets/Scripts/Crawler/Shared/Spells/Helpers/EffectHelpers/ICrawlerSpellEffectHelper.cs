using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using System.Threading;
using UnityEngine;

namespace Genrpg.Shared.Spells.Helpers.SpellEffectHelpers
{
    public interface ICrawlerSpellEffectHelper : ISetupDictionaryItem<long>
    {
        Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token);
    }
}


