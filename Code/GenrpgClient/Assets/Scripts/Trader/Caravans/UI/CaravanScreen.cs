using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Currencies.UI;
using Assets.Scripts.Trader.Travel.UI;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Caravans.UI
{
    public class CaravanScreen : BaseScreen
    {
        private ICaravanService _caravanService = null;
        private ICurrencySpendService _spendService = null;
        private ICalcAttributeService _attributeService = null;
        private IClientWebService _webService = null;

        public GameObject CaravanAnchor;
        public GameObject HoldingsAnchor;

        public CaravanMemberIcon MemberIconPrefab;

        private List<CaravanMemberIcon> _allIcons = new List<CaravanMemberIcon>();
        private List<CaravanMemberIcon> _caravanIcons = new List<CaravanMemberIcon>();
        private List<CaravanMemberIcon> _holdingsIcons = new List<CaravanMemberIcon>();

        public TravelInfoUI TravelUI;

        List<CurrentCaravanMember> oldCaravanMembers = new List<CurrentCaravanMember>();


        public GButton CancelButton;
        public GButton ResetButton;
        public SpendCurrencyButton UpdateMembersSpendButton;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            oldCaravanMembers = new List<CurrentCaravanMember>(caravanData.CurrentMembers);

            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateTraderHUD, GetToken());
            _dispatcher.AddListener<UpdateCaravanMembersResponse>(OnUpdateCaravanMembersResponse, GetToken());

            FullSpendLocation specialSpendLoc = await _spendService.GetFullSpendLocation(_gs.ch, SpendLocations.SpecialActions, true);

            SpendType moveSpendType = null;

            if (specialSpendLoc.IsValid)
            {
                moveSpendType = specialSpendLoc.SpendTypes.FirstOrDefault(x => x.Rewards.Any(x => x.EntityTypeId == EntityTypes.UpdateCaravanMembers));
            }

            if (moveSpendType == null)
            {
                _clientEntityService.SetActive(UpdateMembersSpendButton, false);
            }
            else
            {
                UpdateMembersSpendButton.SetSpendType(specialSpendLoc.Location, moveSpendType, OnClickUpdateMembers);
            }
            _uiService.SetButton(ResetButton, GetName(), ResetMembersAsync);
            _uiService.SetButton(CancelButton, GetName(), ClickCancelAsync);
            await ShowMembers();
            await Task.CompletedTask;
        }

        private void OnUpdateTraderHUD(UpdateTraderHUD update)
        {
            _awaitableService.ForgetAwaitable(ShowMembers());
        }


        private void OnUpdateCaravanMembersResponse(UpdateCaravanMembersResponse response)
        {
            if (response.Success)
            {
                CaravanData cdata = _gs.ch.Get<CaravanData>();
                cdata.CurrentMembers = response.CurrentMembers;
                oldCaravanMembers = response.CurrentMembers;
                _dispatcher.Dispatch(new ShowFloatingText("Caravan Members Updated!"));
            }
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

            City city = pos.GetCurrentCity();

            foreach (CaravanMember cmember in allCaravanMembers)
            {
                if (caravanData.CurrentMembers.Any(x => x.CaravanMemberId == cmember.IdKey))
                {
                    initIconList.Add(new CaravanMemberInitIconData()
                    {
                        InCaravan = true,
                        CaravanMember = cmember,
                        Screen = this,
                    });
                }
                else if (holdingsData.CaravanMembersOwned.HasBitIndex(cmember.IdKey))
                {
                    initIconList.Add(new CaravanMemberInitIconData()
                    {
                        InCaravan = false,
                        CaravanMember = cmember,
                        Screen = this,
                    });
                }
            }

            int siblingIndex = 0;
            _caravanIcons.Clear();
            _holdingsIcons.Clear();
            foreach (CaravanMemberInitIconData iconData in initIconList)
            {

                List<CaravanMemberIcon> currList = null;
                GameObject currParent = null;

                if (iconData.InCaravan)
                {
                    currList = _caravanIcons;
                    currParent = CaravanAnchor;
                }
                else
                {
                    currList = _holdingsIcons;
                    currParent = HoldingsAnchor;
                }

                CaravanMemberIcon currIcon = _allIcons.FirstOrDefault(x => x.GetCaravanMemberId() == iconData.CaravanMember.IdKey);

                if (currIcon != null)
                {
                    currIcon.SetData(iconData, siblingIndex++);
                    if (currIcon.transform.parent != currParent)
                    {
                        _clientEntityService.AddToParent(currIcon, currParent);
                    }
                    currList.Add(currIcon);

                    continue;
                }

                currIcon = _clientEntityService.FullInstantiate(MemberIconPrefab);
                _clientEntityService.AddToParent(currIcon, currParent);
                currList.Add(currIcon);
                currIcon.SetData(iconData, siblingIndex++);
                _allIcons.Add(currIcon);

            }

            _caravanIcons = _caravanIcons.OrderBy(x => x.GetSiblingIndex()).ToList();

            _clientEntityService.ReorderSiblings(_caravanIcons);

            _holdingsIcons = _holdingsIcons.OrderBy(x => x.GetSiblingIndex()).ToList();


            _clientEntityService.ReorderSiblings(_holdingsIcons);
        }

        public async Awaitable ClickCancelAsync(CancellationToken token)
        {
            StartClose();
        }

        public void MoveCaravanMember(long caravanMemberId)
        {
            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            CurrentCaravanMember currentMember = caravanData.CurrentMembers.FirstOrDefault(x=>x.CaravanMemberId == caravanMemberId);

            if (currentMember != null)
            {
                caravanData.CurrentMembers.Remove(currentMember);
            }
            else
            {
                caravanData.CurrentMembers.Add(new CurrentCaravanMember() { CaravanMemberId = caravanMemberId });
            }

            _awaitableService.ForgetAwaitable(UpdateAndShowData());
        }

        private async Awaitable ResetMembersAsync(CancellationToken token)
        {
            CaravanData caravanData = _gs.ch.Get<CaravanData>();
            caravanData.CurrentMembers = new List<CurrentCaravanMember>(oldCaravanMembers);

            await UpdateAndShowData();
        }

        private bool OnClickUpdateMembers(SpendCurrencyRequest request)
        {
            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            List<long> currentMembers = caravanData.CurrentMembers.Select(x => x.CaravanMemberId).OrderBy(x => x).ToList();
            List<long> oldMembers = oldCaravanMembers.Select(x => x.CaravanMemberId).OrderBy(x => x).ToList();

            bool haveDifference = false;

            if (currentMembers.Count != oldMembers.Count)
            {
                haveDifference = true;
            }
            else
            {
                for (int i = 0; i < currentMembers.Count; i++)
                {
                    if (currentMembers[i] != oldMembers[i])
                    {
                        haveDifference = true;
                        break;
                    }
                }
            }

            if (!haveDifference)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You haven't made any changes.", EFloatingTextArt.Message));

                return false;
            }


            if (caravanData.CurrentMembers.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int m = 0; m < caravanData.CurrentMembers.Count; m++)
                {
                    sb.Append(caravanData.CurrentMembers[m].CaravanMemberId);
                    if (m < caravanData.CurrentMembers.Count - 1)
                    {
                        sb.Append("|");
                    }
                }

                request.ExtraRewardArgs = sb.ToString();
            }
            else
            {
                request.ExtraRewardArgs = CaravanMemberConstants.EmptyMemberListString;
            }

            return true;
        }


        protected override void OnStartClose()
        {
            CaravanData cdata = _gs.ch.Get<CaravanData>();

            cdata.CurrentMembers = new List<CurrentCaravanMember>(oldCaravanMembers);

            _awaitableService.ForgetAwaitable(UpdateBuffs());
        }
        
        private async Awaitable UpdateAndShowData()
        {
            await UpdateBuffs();
            await ShowMembers();
        }

        private async Awaitable UpdateBuffs()
        {
            await _attributeService.CalcAllAttributes(_gs.ch, false);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }
    }
}


