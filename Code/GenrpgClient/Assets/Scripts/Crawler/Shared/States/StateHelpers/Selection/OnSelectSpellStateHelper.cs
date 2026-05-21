
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Entities;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Combat;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Spells.Constants;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Selection
{
    public class OnSelectSpellStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.OnSelectSpell;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Select Spell Action missing on select" };
            }

            PartyData party = _crawlerService.GetParty();

            Item castingItem = null;

            if (selectSpellAction.Action.Action != null)
            {
                castingItem = selectSpellAction.Action.Action.CastingItem;
            }

            UnitAction newAction = _combatService.GetActionFromSpell(party, selectSpellAction.Action.Member,
                selectSpellAction.Spell, null, castingItem);

            if (newAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Failed to create action after selecting spell" };
            }

            selectSpellAction.Action.Action = newAction;
            selectSpellAction.Action.Member.AddAction(newAction);

            if (newAction.Spell.TargetTypeId == TargetTypes.Special)
            {
                return new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true) { ExtraData = selectSpellAction };
            }

            ECrawlerStates nextState = selectSpellAction.Action.NextState;
            if (newAction.FinalTargets.Count < 1)
            {
                if (newAction.PossibleTargetGroups.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectEnemyGroup, true) { ExtraData = selectSpellAction };
                }
                else if (newAction.PossibleTargetUnits.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectAllyTarget, true) { ExtraData = selectSpellAction };
                }
                else
                {
                    return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Selected spell but no targets available" };
                }
            }

            await Task.CompletedTask;
            return new CrawlerStateData(nextState, true) { ExtraData = selectSpellAction };
        }
    }
}


