using OxDb.RequestServer.Core;
using OxDb.SharedGame.Characters.PlayerData;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public abstract class BaseCharacterLoadUpdater : ICharacterLoadUpdater
    {

        public Type HelperKey => GetType();
        public virtual int Order => 0;

        public abstract Task Update(WebContext context, Character ch);

    }
}


