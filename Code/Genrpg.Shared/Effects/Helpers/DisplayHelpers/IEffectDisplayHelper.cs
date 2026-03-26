using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public interface IEffectDisplayHelper : ISetupDictionaryItem<long>
    {
        string DisplayEffect(IFilteredObject obj, IEffect effect);
    }
}


