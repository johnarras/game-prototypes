using Assets.Scripts.Core;
using Assets.Scripts.Crawler.Buffs.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Spells.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.Spells.Helpers.SpellEffectHelpers;
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


