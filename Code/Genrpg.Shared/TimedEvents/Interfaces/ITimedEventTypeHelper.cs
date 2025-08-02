using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.TimedEvents.Entities;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ITimedEventTypeHelper
    {
        long UserCoinTypeId { get; }

        ICurrentTimedEventSettings GetCurrent(IFilteredObject obj);
        ITimedEventTierSettings GetTierSettings(IFilteredObject obj);
        ITimedEventTierList GetTierList(IFilteredObject obj);
        ITimedEventThemeSettings GetThemeSettings(IFilteredObject obj);
        ITimedEventTheme GetTheme(IFilteredObject obj);
    }
}
