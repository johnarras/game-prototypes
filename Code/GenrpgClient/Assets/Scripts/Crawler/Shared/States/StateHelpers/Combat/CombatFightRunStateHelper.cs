using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Combat
{
    public class CombatFightRunStateHelper : BaseCombatStateHelper
    {
        private ICrawlerMoveService _moveService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.CombatFightRun;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
                    _dispatcher.Dispatch(new ShowWorldPanelImage(group.Units[0].PortraitName));
                    stateData.WorldSpriteName = group.Units[0].PortraitName;
                    didShowPortrait = true;
                }

                stateData.Actions.Add(new CrawlerStateAction(_combatService.ShowGroupStatus(group),
                    pointerEnterAction: (GameObject go) => { ShowInfo(EntityTypes.Unit, group.UnitType.IdKey); }));
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
                    pointerEnterAction: (GameObject go) => { ShowInfo(EntityTypes.Unit, group.UnitType.IdKey); }));

            }


            stateData.AddBlankLine();

            if (party.Combat.RoundsComplete == 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Prepare", Key.P, ECrawlerStates.CombatPlayer,
                       onClickAction: delegate ()
                       {
                           party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Prepare;
                           _combatService.InitPartyCombatActions(party);
                       }));
            }

            stateData.Actions.Add(new CrawlerStateAction("Fight", Key.F, ECrawlerStates.CombatPlayer,
                   onClickAction: delegate ()
                   {
                       party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Fight;
                       _combatService.InitPartyCombatActions(party);
                   }));



            if (!_optionsService.HasOption(party, CrawlerOptions.Permadeath))
            {

                stateData.Actions.Add(new CrawlerStateAction("Run", Key.R, ECrawlerStates.CombatConfirm,
                    onClickAction: delegate ()
                    {
                        party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Run;
                    }));
            }

            long minRange = CrawlerCombatConstants.MaxRange;

            foreach (CombatGroup group in party.Combat.Enemies)
            {
                minRange = Math.Min(minRange, group.Range);
            }

            if (minRange > CrawlerCombatConstants.MinRange)
            {
                stateData.Actions.Add(new CrawlerStateAction("Charge", Key.C, ECrawlerStates.CombatConfirm,
               onClickAction: delegate ()
               {
                   party.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Charge;
               }));
            }


            _moveService.ClearMovement();

            await Task.CompletedTask;
            return stateData;
        }
    }
}


