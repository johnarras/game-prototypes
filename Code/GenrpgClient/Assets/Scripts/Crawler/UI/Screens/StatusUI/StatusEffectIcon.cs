using OxDb.Client.Entities.UI;
using OxDb.SharedCore.Entities.Constants;

namespace OxDb.Client.Crawler.UI.StatusUI
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


