using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public interface IUserLoadUpdater : IOrderedSetupDictionaryItem<Type>
    {
        Task Update(WebContext context, List<IUnitData> unitData);
    }
}


