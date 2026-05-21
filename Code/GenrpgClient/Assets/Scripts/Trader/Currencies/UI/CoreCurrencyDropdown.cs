using Assets.Scripts.UI.Entities;
using OxDb.SharedGame.Currencies.Settings;

namespace Assets.Scripts.Trader.UI.Currencies
{
    public class CoreCurrencyDropdown : TypedEntityIdDropdownScript<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override bool OrderByName()
        {
            return true;
        }
    }
}


