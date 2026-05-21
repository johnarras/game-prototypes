using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.RequestServer.PlayerData.LoadUpdateHelpers
{
    public interface IUserLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        Task Update(WebContext context, List<IUnitData> unitData);
    }
}


