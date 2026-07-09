using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class PartyBuffEffectCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.PartyBuff;

        public override async ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {

            double tier = _roleService.GetRoleScalingLevel(party, caster, spell.Spell.RoleScalingTypeId);

            party.Buffs[fullEffect.Effect.EntityId] = _buffService.GetPartyBuffPower(party, fullEffect.Effect.EntityId);
            _dispatcher.Dispatch(new UpdateCrawlerUI());
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });


            await Task.CompletedTask;
        }
    }
}


