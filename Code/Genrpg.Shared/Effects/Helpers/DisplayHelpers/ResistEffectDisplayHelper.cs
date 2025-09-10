using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{

    public class ResistEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.Resist;

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
