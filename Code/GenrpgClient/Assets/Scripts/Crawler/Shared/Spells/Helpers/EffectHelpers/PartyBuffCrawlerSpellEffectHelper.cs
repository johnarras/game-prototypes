using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Spells.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class PartyBuffEffectCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        protected IRoleService _roleService = null;
        public override long HelperKey => EntityTypes.PartyBuff;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {

            double tier = _roleService.GetRoleScalingLevel(party, caster, RoleScalingTypes.Utility);

            party.Buffs[fullEffect.Effect.EntityId] = _buffService.GetPartyBuffPower(party, fullEffect.Effect.EntityId);
            _dispatcher.Dispatch(new UpdateCrawlerUI());
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });


            await Task.CompletedTask;
        }
    }
}


