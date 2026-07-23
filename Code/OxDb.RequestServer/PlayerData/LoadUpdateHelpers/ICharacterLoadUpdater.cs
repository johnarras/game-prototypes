using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public enum ECharacterLoadUpdateOrder
    {
        Core = 1,
        Inputs = 2,
        Spells = 3,
        Charms = 4,
    }


    public interface ICharacterLoadUpdater : IOrderedSetupDictionaryItem<ECharacterLoadUpdateOrder>
    {
        Task Update(WebContext context, Character ch);
    }
}


