using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Settings;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public class MapMagicEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.MapMagic;

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


