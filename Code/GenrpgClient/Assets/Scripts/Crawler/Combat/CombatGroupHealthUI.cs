using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.UnitEffects.Constants;
using System.Collections.Generic;

namespace OxDb.Client.Crawler.Combat
{
    public class CombatGroupHealthUI : BaseBehaviour
    {
        public List<GroupHealthIcon> Icons;

        public void UpdateHealthData(CombatGroup group)
        {
            int iconsShown = 0;

            if (group != null)
            {
                for (int i = 0; i < group.Units.Count; i++)
                {
                    if (iconsShown >= Icons.Count)
                    {
                        break;
                    }

                    if (!group.Units[i].StatusEffects.HasBitIndex(StatusEffects.Dead))
                    {
                        Icons[i].UpdateFromUnit(group.Units[i]);
                        iconsShown++;
                    }
                }
            }

            for (int i = iconsShown; i < Icons.Count; i++)
            {
                Icons[i].UpdateFromUnit(null);
            }
        }
    }
}
