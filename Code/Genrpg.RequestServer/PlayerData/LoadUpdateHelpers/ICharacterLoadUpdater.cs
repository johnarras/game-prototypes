using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public interface ICharacterLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        Task Update(WebContext context, Character ch);
    }
}


