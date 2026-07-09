using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Procs.Entities;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    public class StatEffectHandler : BaseSpellEffectHandler
    {
        public override long HelperKey => EntityTypes.Stat;
        public override bool IsModifyStatEffect() { return true; }
        public override bool UseStatScaling() { return true; }

        public override List<ActiveSpellEffect> CreateEffects(MapObject obj, SpellHit hitData)
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

                if (obj.Rand.NextDouble() > proc.Chance)
                {
                    continue;
                }

                ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
                eff.EntityTypeId = EntityTypes.Stat;
                eff.EntityId = proc.EntityId;
                eff.Quantity = RandUtils.LongRange(proc.MinQuantity, proc.MaxQuantity, obj.Rand);
                retval.Add(eff);
            }
            return retval;
        }

        public override bool HandleEffect(MapObject obj, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}


