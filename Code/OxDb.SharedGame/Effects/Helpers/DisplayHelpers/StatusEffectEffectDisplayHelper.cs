using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.UnitEffects.Settings;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public class StatusEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.StatusEffect;

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


