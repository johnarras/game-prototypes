using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Combat;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Selection
{
    public class SelectEnemyGroupStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.SelectEnemyGroup;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select enemies out of combat" };
            }

            SelectSpellAction selectAction = action.ExtraData as SelectSpellAction;

            if (selectAction == null || selectAction.Action == null || selectAction.Action.Action == null ||
                selectAction.Action.Member == null ||
                selectAction.Action.Action.PossibleTargetGroups.Count < 1)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "No enemy groups to select" };
            }

            CrawlerUnit currUnit = selectAction.Action.Member;

            for (int m = 0; m < selectAction.Action.Action.PossibleTargetGroups.Count; m++)
            {
                CombatGroup group = selectAction.Action.Action.PossibleTargetGroups[m];
                char c = (char)('A' + m);

                Action clickRowAction = delegate ()
                {
                    selectAction.Action.Action.FinalTargets = group.Units.ToList();
                    currUnit.AddAction(selectAction.Action.Action);
                    selectAction.Action.Action.FinalTargetGroups = new List<CombatGroup>() { group };
                    _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
                };

                CrawlerStateAction newAction = new CrawlerStateAction(char.ToUpper(c) + " " + _combatService.ShowGroupStatus(group), FromChar(c),
                    selectAction.Action.NextState, onClickAction: clickRowAction, forceButton: false,
                    pointerEnterAction: (GameObject go) => { ShowInfo(EntityTypes.Unit, group.UnitType.IdKey); });

                stateData.Actions.Add(newAction);

                Action clickIconAction =
                    delegate
                    {
                        _crawlerService.ChangeState(stateData, newAction, _crawlerService.GetToken());
                    };


                _dispatcher.Dispatch(new SetSelectEnemyGroupAction() { Action = clickIconAction, Group = group });

            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.CombatPlayer,
                delegate ()
                {
                    _dispatcher.Dispatch(new ClearSelectCrawlerUnitActions());
                }));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


