using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{

    public enum EUserLoadUpdateOrder
    {
        Core = 1,
    }

    public interface IUserLoadUpdater : IOrderedSetupDictionaryItem<EUserLoadUpdateOrder>
    {
        Task Update(WebContext context, List<IUnitData> unitData);
    }
}


