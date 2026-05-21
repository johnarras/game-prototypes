using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{

    public class RiddleEffectEffectDisplayHelper : BaseEffectDisplayHelper
    {
        public override long HelperKey => EntityTypes.Riddle;

        public override string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            return "Ignore Riddles";
        }
    }
}


