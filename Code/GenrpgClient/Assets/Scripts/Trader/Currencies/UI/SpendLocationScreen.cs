using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.CaravanMembers.Services;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Cultures.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Trader.Currencies.UI
{
    public class SpendLocationScreen : BaseScreen
    {
        protected ICurrencySpendService _spendService = null;
        protected ICaravanService _caravanService = null;

        public EntityTypeWithIdUI LocationEntity;

        public SpendTypeIcon IconPrefab;

        public GameObject IconAnchor;

        public GText Header;
        public GText Desc;
        public GText Message;

        protected List<SpendTypeIcon> _icons = new List<SpendTypeIcon>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await ShowPurchaseItems();
            _dispatcher.AddListener<SpendCurrencyResponse>(OnSpendCurrency, GetToken());
            await Task.CompletedTask;
        }

        protected virtual void OnSpendCurrency(SpendCurrencyResponse response)
        {
            _awaitableService.ForgetAwaitable(ShowPurchaseItems());
        }

        protected virtual async Awaitable ShowPurchaseItems()
        {

            FullSpendLocation fullSpendLocation = await _spendService.GetFullSpendLocation(_gs.ch, LocationEntity.EntityId, true);

            if (fullSpendLocation == null || fullSpendLocation.Location == null)
            {
                _clientEntityService.DestroyAllChildren(IconAnchor);
                return;
            }


            _uiService.SetText(Header, fullSpendLocation.Location.Name);

            CoreData coreData = _gs.ch.Get<CoreData>();


            string desc = fullSpendLocation.Location.Desc;


            CaravanPosition pos = _caravanService.GetPosition(coreData);

            City namedCity = null;

            if (pos.GetCurrentCity() != null)
            {
                namedCity = pos.GetCurrentCity();
            }
            else if (pos.TargetCity != null)
            {
                namedCity = pos.TargetCity; 
            }

            if (namedCity != null)
            {
                CultureType cultureType = _gameData.Get<CultureTypeSettings>(_gs.ch).Get(namedCity.CultureTypeId);

                if (cultureType != null)
                {
                    desc += " in the " + cultureType.Name;
                }

                desc += " city of " + namedCity.Name;
            }

            _uiService.SetText(Desc, desc);

            if (!fullSpendLocation.IsValid)
            {
                _clientEntityService.DestroyAllChildren(IconAnchor);
                return;
            }

            List<SpendType> currentSpendTypes = fullSpendLocation.SpendTypes;

            foreach (SpendType spendType in currentSpendTypes)
            {
                SpendTypeIcon currIcon = _icons.FirstOrDefault(x => x.GetSpendTypeIndex() == spendType.Index);

                if (currIcon == null)
                {
                    currIcon = _clientEntityService.FullInstantiate(IconPrefab);
                    _clientEntityService.AddToParent(currIcon, IconAnchor);
                    currIcon.SetData(fullSpendLocation.Location, spendType);
                    _icons.Add(currIcon);
                }
            }

            List<SpendTypeIcon> removeList = new List<SpendTypeIcon>();
            foreach (SpendTypeIcon icon in _icons)
            {
                if (!currentSpendTypes.Any(x => x.Index == icon.GetSpendTypeIndex()))
                {
                    removeList.Add(icon);
                }
            }

            foreach (SpendTypeIcon icon in removeList)
            {
                _clientEntityService.Destroy(icon);
                _icons.Remove(icon);
            }

            _icons = _icons.OrderBy(x => x.GetSpendTypeIndex()).ToList();
            _clientEntityService.ReorderSiblings(_icons);

        }
    }
}
