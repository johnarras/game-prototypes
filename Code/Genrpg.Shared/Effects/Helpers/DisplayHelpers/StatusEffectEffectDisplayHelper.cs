using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UnitEffects.Settings;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public class StatusEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.StatusEffect;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(null).Get(effect.EntityId);
            if (statusEffect != null)
            {
                return "Immune to " + statusEffect.Name;
            }
            return null;
        }
    }
}
