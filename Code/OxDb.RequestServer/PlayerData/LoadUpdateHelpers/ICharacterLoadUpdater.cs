using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public interface ICharacterLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        Task Update(WebContext context, Character ch);
    }
}


