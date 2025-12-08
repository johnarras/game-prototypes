using Assets.Scripts.Crawler.Buffs.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Helpers.SpellEffectHelpers;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public abstract class BaseCrawlerSpellEffectHelper : ICrawlerSpellEffectHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        protected IDispatcher _dispatcher = null;
        protected IBuffService _buffService = null;
        protected IPartyService _partyService = null;
        protected ICrawlerSpellService _spellService = null;
        protected ICrawlerStatService _crawlerStatService = null;
        protected ICrawlerCombatService _combatService = null;

        public abstract long HelperKey { get; }

        public abstract Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token);
    }
}
