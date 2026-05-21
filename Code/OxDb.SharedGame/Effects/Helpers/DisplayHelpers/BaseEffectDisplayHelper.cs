using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.Effects.Helpers.DisplayHelpers
{
    public abstract class BaseEffectDisplayHelper : IEffectDisplayHelper
    {

        protected IGameData _gameData = null;

        public abstract long HelperKey { get; }
        public abstract string DisplayEffect(IFilteredObject obj, IEffect effect);

    }
}


