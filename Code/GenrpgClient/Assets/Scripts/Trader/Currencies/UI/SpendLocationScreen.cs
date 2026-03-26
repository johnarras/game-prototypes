using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
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

        public EntityTypeWithIdUI LocationEntity;


        public SpendTypeIcon IconPrefab;

        public GameObject IconAnchor;

        public GText Header;
        public GText Desc;

        protected List<SpendTypeIcon> _icons = new List<SpendTypeIcon>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await ShowPurchaseItems();
            await Task.CompletedTask;
            UIDocument doc;
        }

        virtual protected async Awaitable ShowPurchaseItems()
        {

            FullSpendLocation fullSpendLocation = await _spendService.GetFullSpendLocation(_gs.ch, LocationEntity.EntityId, true);

            if (fullSpendLocation == null || fullSpendLocation.Location == null)
            {
                _clientEntityService.DestroyAllChildren(IconAnchor);
                return;
            }

            _uiService.SetText(Header, fullSpendLocation.Location.Name);
            _uiService.SetText(Desc, fullSpendLocation.Location.Desc);

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
                if (!currentSpendTypes.Any(x=>x.Index == icon.GetSpendTypeIndex()))
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
