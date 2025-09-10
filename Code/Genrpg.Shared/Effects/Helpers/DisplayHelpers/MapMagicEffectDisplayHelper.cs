using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public class MapMagicEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.MapMagic;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            MapMagicType mtype = _gameData.Get<MapMagicSettings>(null).Get(effect.EntityId);
            if (mtype != null)
            {
                return "Ignore " + mtype.Name + " Squares";
            }

            return null;
        }
    }
}
