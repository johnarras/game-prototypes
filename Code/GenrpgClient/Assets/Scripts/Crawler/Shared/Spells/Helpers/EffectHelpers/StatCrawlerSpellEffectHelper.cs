using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class StatCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Stat;

        public override async Awaitable ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {
            if (args.CurrHitTimes > 0 || fullEffect.Effect.MaxQuantity < 1 || party.Combat == null)
            {
                return;
            }

            StatVal statVal = party.Combat.StatBuffs.FirstOrDefault(x => x.StatTypeId == fullEffect.Effect.EntityId);
            if (statVal == null)
            {
                statVal = new StatVal()
                {
                    StatTypeId = (short)fullEffect.Effect.EntityId,
                };
                party.Combat.StatBuffs.Add(statVal);
            }

            if (statVal.Val < caster.Level)
            {
                statVal.Val = (int)caster.Level;
                _crawlerStatService.CalcPartyStats(party, false);
            }


            await Task.CompletedTask;
        }
    }
}


