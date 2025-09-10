using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{

    public class RiddleEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long Key => EntityTypes.Riddle;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            return "Ignore Riddles";
        }
    }
}
