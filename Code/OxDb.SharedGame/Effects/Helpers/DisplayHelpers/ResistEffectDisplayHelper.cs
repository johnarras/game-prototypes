using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Spells.Settings.Elements;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{

    public class ResistEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.Resist;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            ElementType elementType = _gameData.Get<ElementTypeSettings>(null).Get(effect.EntityId);
            if (elementType != null)
            {
                return "Resistant to " + elementType.Name;
            }
            return null;
        }
    }
}


