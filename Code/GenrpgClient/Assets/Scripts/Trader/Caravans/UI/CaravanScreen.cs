using Assets.Scripts.Trader.UI.Icons;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Caravans.UI
{
    public class CaravanScreen : BaseScreen
    {
        private ICaravanService _caravanService = null;
        private ICurrencySpendService _spendService = null;

        public GameObject MemberAnchor;
        public GameObject TradeGoodAnchor;

        public CaravanMemberIcon MemberIconPrefab;
        public TradeGoodIcon TradeGoodPrefab;

        private List<CaravanMemberIcon> _memberIcons = new List<CaravanMemberIcon>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            await ShowMembers();
            await Task.CompletedTask;
        }

        private async Awaitable ShowMembers()
        {

            CaravanMemberSettings memberSettings = _gameData.Get<CaravanMemberSettings>(_gs.ch);
            IReadOnlyList<CaravanMember> allCaravanMembers = memberSettings.GetData();

            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            HoldingsData holdingsData = _gs.ch.Get<HoldingsData>();

            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            List<CaravanMemberInitIconData> initIconList = new List<CaravanMemberInitIconData>();

            FullSpendLocation memberSpendLoc = await _spendService.GetFullSpendLocation(_gs.ch, SpendLocations.CaravanMembers, true);

            City city = pos.GetCurrentCity();

            foreach (CaravanMember cmember in allCaravanMembers)
            {
                CaravanMemberInitIconData iconData = new CaravanMemberInitIconData()
                {
                    CaravanMember = cmember,
                    SpendLoc = memberSpendLoc.Location,
                    CurrentCity = city, 
                };
                initIconList.Add(iconData);

                if (caravanData.CurrentMembers.Any(x => x.CaravanMemberId == cmember.IdKey))
                {
                    iconData.CurrentLocation = ECaravanMemberLocations.Caravan;
                    if (city != null)
                    {
                        iconData.TargetLocation = ECaravanMemberLocations.Holdings;
                    }
                    else
                    {
                        iconData.TargetLocation = ECaravanMemberLocations.None;
                    }
                }
                else if (holdingsData.CaravanMembersOwned.HasBitIndex(cmember.IdKey))
                {
                    iconData.CurrentLocation = ECaravanMemberLocations.Holdings;

                    if (city != null)
                    {
                        iconData.TargetLocation = ECaravanMemberLocations.Caravan;
                    }
                    else
                    {
                        iconData.TargetLocation = ECaravanMemberLocations.None;
                    }
                }
                else
                {
                    bool addedToVendor = false;
                    if (memberSpendLoc.IsValid)
                    {
                        iconData.SpendType = memberSpendLoc.SpendTypes.FirstOrDefault(x=>x.Rewards.Any(y=>y.EntityTypeId == EntityTypes.CaravanMember &&
                        y.EntityId == cmember.IdKey));

                        if (iconData.SpendType != null)
                        {
                            iconData.CurrentLocation = ECaravanMemberLocations.Vendor;
                            iconData.TargetLocation = ECaravanMemberLocations.Holdings;
                            addedToVendor = true;
                        }
                    }

                    if (!addedToVendor)
                    {
                        iconData.CurrentLocation = ECaravanMemberLocations.Unavailable;
                        iconData.TargetLocation = ECaravanMemberLocations.None;
                    }
                }
            }

            initIconList = initIconList.OrderBy(x => x.CurrentLocation).ToList();

            int siblingIndex = 0;
            foreach (CaravanMemberInitIconData iconData in initIconList)
            {
                if (iconData.CurrentLocation == ECaravanMemberLocations.Unavailable ||
                    iconData.CurrentLocation == ECaravanMemberLocations.None)
                {
                    continue;
                }

                CaravanMemberIcon currIcon = _memberIcons.FirstOrDefault(x => x.GetCaravanMemberId() == iconData.CaravanMember.IdKey);

                if (currIcon != null)
                {
                    currIcon.SetData(iconData, siblingIndex++);
                    continue;
                }

                currIcon = _clientEntityService.FullInstantiate(MemberIconPrefab);              
                _clientEntityService.AddToParent(currIcon, MemberAnchor);
                _memberIcons.Add(currIcon);
                currIcon.SetData(iconData, siblingIndex++);
            }

            _memberIcons = _memberIcons.OrderBy(x => x.GetSiblingIndex()).ToList();

            _clientEntityService.ReorderSiblings(_memberIcons);

        }
    }
}


