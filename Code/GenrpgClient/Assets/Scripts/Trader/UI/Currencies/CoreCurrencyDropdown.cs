using Assets.Scripts.UI.Entities;
using Genrpg.Shared.CoreCurrencies.Settings;

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
