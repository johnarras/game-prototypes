using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public interface IEffectDisplayHelper : ISetupDictionaryItem<long>
    {
        string DisplayEffect(IFilteredObject obj, IEffect effect);
    }
}


