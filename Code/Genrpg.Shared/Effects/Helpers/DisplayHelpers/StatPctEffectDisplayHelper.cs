using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public class StatPctEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.StatPct;

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
