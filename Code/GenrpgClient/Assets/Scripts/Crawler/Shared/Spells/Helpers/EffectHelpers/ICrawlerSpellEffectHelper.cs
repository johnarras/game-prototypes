using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spells.Helpers.SpellEffectHelpers
{
    public interface ICrawlerSpellEffectHelper : ISetupDictionaryItem<long>
    {
        ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token);
    }
}


