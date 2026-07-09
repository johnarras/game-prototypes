using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Entities;
using OxDb.SharedGame.Stats.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.Spells.Helpers.EffectHelpers
{
    public class StatCrawlerSpellEffectHelper : BaseCrawlerSpellEffectHelper
    {
        public override long HelperKey => EntityTypes.Stat;

        public override async ValueTask ApplyEffectToUnit(PartyData party, ApplyEffectArgs args, FullSpell spell, FullEffect fullEffect, CrawlerUnit caster, CrawlerUnit target, CancellationToken token)
        {
            if (args.CurrHitTimes > 0 || fullEffect.Effect.StatBonusDamageScale < 1 || party.Combat == null)
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


