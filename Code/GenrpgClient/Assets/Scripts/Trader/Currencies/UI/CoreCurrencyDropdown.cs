using OxDb.Client.UI.Entities;
using OxDb.SharedGame.Currencies.Settings;

namespace OxDb.Client.Trader.UI.Currencies
{
    public class CoreCurrencyDropdown : TypedEntityIdDropdownScript<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override bool OrderByName()
        {
            return true;
        }
    }
}


