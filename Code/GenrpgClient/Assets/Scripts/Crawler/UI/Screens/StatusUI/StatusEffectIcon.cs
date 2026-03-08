using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Entities.Constants;

namespace Assets.Scripts.Crawler.UI.StatusUI
{
    public class StatusEffectIcon : EntityIcon
    {
        public long GetStatusEffectId()
        {
            return _entityId;
        }

        public void SetData(long statusEffectId)
        {
            SetEntityData(EntityTypes.StatusEffect, statusEffectId, 1, 1);
        }
    }
}


