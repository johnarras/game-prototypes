using OxDb.RequestServer.Core;
using OxDb.SharedGame.Characters.PlayerData;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public abstract class BaseCharacterLoadUpdater : ICharacterLoadUpdater
    {
        public abstract ECharacterLoadUpdateOrder HelperKey { get; }

        public abstract Task Update(WebContext context, Character ch);
    }
}


