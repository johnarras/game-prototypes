using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
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

            party.Buffs.Set(fullEffect.Effect.EntityId, _buffService.GetPartyBuffPower(party, fullEffect.Effect.EntityId)); ;
            _dispatcher.Dispatch(new UpdateCrawlerUI());
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });


            await Task.CompletedTask;
        }
    }
}
