using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Helpers;
using Genrpg.Shared.TimedEvents.Collections.Settings;

namespace Genrpg.Shared.TimedEvents.Collections.Helpers
{
    public class CollectionTimedActivityHelper : BaseActivityHelper<CurrentCollectionSettings, CollectionThemeSettings, CollectionTheme,
        CollectionTierListSettings, CollectionTierList>
    {
        public override long UserCoinTypeId => TimedEventCurrencyTypes.Collection;

        public override long Key => TimedEventTypes.Collection;
    }
}
