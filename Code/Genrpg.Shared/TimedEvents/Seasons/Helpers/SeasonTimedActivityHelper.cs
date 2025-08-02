using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Helpers;
using Genrpg.Shared.TimedEvents.Seasons.Settings;

namespace Genrpg.Shared.TimedEvents.Seasons.Helpers
{
    public class SeasonTimedActivityHelper : BaseActivityHelper<CurrentSeasonSettings, SeasonThemeSettings, SeasonTheme,
        SeasonTierListSettings, SeasonTierList>
    {
        public override long UserCoinTypeId => TimedEventCurrencyTypes.Season;

        public override long Key => TimedEventTypes.Season;
    }
}
