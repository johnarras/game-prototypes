using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Spells.Settings.Elements;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{

    public class ElementEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.Element;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            ElementType elementType = _gameData.Get<ElementTypeSettings>(null).Get(effect.EntityId);
            if (elementType != null)
            {
                return "Ignore " + elementType.Name + " Resist";
            }
            return null;
        }
    }
}


