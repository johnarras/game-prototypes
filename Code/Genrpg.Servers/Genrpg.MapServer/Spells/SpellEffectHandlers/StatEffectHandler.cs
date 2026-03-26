using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class StatEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => EntityTypes.Stat;
        public override bool IsModifyStatEffect() { return true; }
        public override bool UseStatScaling() { return true; }

        public override List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit hitData)
        {

            List<ActiveSpellEffect> retval = new List<ActiveSpellEffect>();

            long target = hitData.SkillType.TargetTypeId;

            List<SpellProc> list = hitData.ElementType.Procs.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

            if (target == TargetTypes.Enemy)
            {
                list = list.Where(x => x.MaxQuantity < 0).ToList();
            }
            else
            {
                list = list.Where(x => x.MinQuantity > 0).ToList();
            }

            if (list == null)
            {
                return retval;
            }

            foreach (SpellProc proc in list)
            {

                if (rand.NextDouble() > proc.Chance)
                {
                    continue;
                }

                ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
                eff.EntityTypeId = EntityTypes.Stat;
                eff.EntityId = proc.EntityId;
                eff.Quantity = RandUtils.LongRange(proc.MinQuantity, proc.MaxQuantity, rand);
                retval.Add(eff);
            }
            return retval;
        }

        public override bool HandleEffect(IRandom rand, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}


