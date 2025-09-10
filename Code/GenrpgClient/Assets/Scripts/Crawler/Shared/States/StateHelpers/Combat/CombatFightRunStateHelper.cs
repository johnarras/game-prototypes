
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class CombatFightRunStateHelper : BaseCombatStateHelper
    {

        private ICrawlerMoveService _moveService;
        private IInfoService _infoService;

        public override ECrawlerStates Key => ECrawlerStates.CombatFightRun;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.BGSpriteName = CrawlerClientConstants.BattlefieldImage;
            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Party is not in combat." };
            }

            bool didShowPortrait = false;
            stateData.AddText("You face: ");

            foreach (CombatGroup group in party.Combat.Enemies)
            {
                if (group.Units.Count < 1 || group.UnitType == null)
                {
                    continue;
                }

                if (!didShowPortrait)
                {
                    _dispatcher.Dispatch(new SetWorldPicture(group.Units[0].PortraitName, false));
                    stateData.WorldSpriteName = group.Units[0].PortraitName;
                    didShowPortrait = true;
                }

                stateData.Actions.Add(new CrawlerStateAction(_combatService.ShowGroupStatus(group),
                    pointerEnterAction: () => { ShowInfo(EntityTypes.Unit, group.UnitType.IdKey); }));
            }

            List<Monster> alliedMonsters = new List<Monster>();

            bool didShowHeader = false;
            foreach (CombatGroup group in party.Combat.Allies)
            {
                if (group == party.Combat.PartyGroup)
                {
                    continue;
                }

                if (group.Units.Count > 0)
                {
                    if (!didShowHeader)
                    {
                        stateData.AddText("Your allies:\n");
                    }
                    didShowHeader = true;
                }

                stateData.Actions.Add(new CrawlerStateAction(_combatService.ShowGroupStatus(group),
                    pointerEnterAction: () => { ShowInfo(EntityTypes.Unit, group.UnitType.IdKey); }));

            }


            stateData.AddBlankLine();

            if (party.Combat.RoundsComplete == 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Prepare", 'P', ECrawlerStates.CombatPlayer,
                       onClickAction: delegate ()
                       {
                           party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Prepare;
                       }));
            }

            stateData.Actions.Add(new CrawlerStateAction("Fight", 'F', ECrawlerStates.CombatPlayer,
                   onClickAction: delegate ()
                   {
                       party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Fight;
                   }));



            //stateData.Actions.Add(new CrawlerStateAction("Run", 'R', ECrawlerStates.CombatConfirm,
            //    onClickAction: delegate ()
            //    {
            //        party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Run;
            //    }));

            long minRange = CrawlerCombatConstants.MaxRange;

            foreach (CombatGroup group in party.Combat.Enemies)
            {
                minRange = Math.Min(minRange, group.Range);
            }

            if (minRange > CrawlerCombatConstants.MinRange)
            {
                stateData.Actions.Add(new CrawlerStateAction("Advance", 'A', ECrawlerStates.CombatConfirm,
               onClickAction: delegate ()
               {
                   party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Advance;
               }));
            }


            _moveService.ClearMovement();

            await Task.CompletedTask;
            return stateData;
        }
    }
}
