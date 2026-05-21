using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Stats.Settings.Stats;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public class StatPctEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.StatPct;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            StatType statType = _gameData.Get<StatSettings>(obj).Get(effect.EntityId);
            if (statType == null)
            {
                return "";
            }

            return effect.Quantity + "% " + statType.Name;
        }
    }
}


