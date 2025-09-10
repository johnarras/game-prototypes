using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Helpers.DisplayHelpers
{
    public abstract class BaseEffectDisplayHelper : IEffectDisplayHelper
    {

        protected IGameData _gameData = null;

        public abstract long Key { get; }
        public abstract string DisplayEffect(IFilteredObject obj, IEffect effect);

    }
}
