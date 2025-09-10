using Assets.Scripts.Crawler.Items.Services;
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Selection
{

    public class SelectUsableItemArgs
    {
        public string MemberId { get; set; }
        public ECrawlerStates ReturnState { get; set; }
        public ECrawlerStates NextState { get; set; }
    }


    public class SelectUsableItemStateHelper : BaseStateHelper
    {

        private ICrawlerItemService _crawlerItemService = null;

        public override ECrawlerStates Key => ECrawlerStates.SelectUsableItem;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectUsableItemArgs data = action.ExtraData as SelectUsableItemArgs;

            if (data == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Invalid use item select data." };
            }

            PartyData party = _crawlerService.GetParty();

            long maxCharges = _crawlerItemService.GetItemUsesPerCombat(party);

            stateData.AddText($"Items can be used {maxCharges} time" + (maxCharges > 1 ? "s" : "") + " between each start of combat.");

            data.NextState = (party.Combat != null ? ECrawlerStates.CombatPlayer : ECrawlerStates.WorldCast);

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);

            List<MemberItemSpell> memberList = new List<MemberItemSpell>();

            List<MemberItemSpell> drainedItems = new List<MemberItemSpell>();

            foreach (PartyMember member in party.Members)
            {
                if (data != null && !string.IsNullOrEmpty(data.MemberId) && member.Id != data.MemberId)
                {
                    continue;
                }
                memberList.AddRange(_crawlerItemService.GetUsableItemsForMember(party, member));
            }

            for (int m = 0; m < memberList.Count; m++)
            {

                MemberItemSpell memberItem = memberList[m];

                if (memberItem.ChargesLeft < 1)
                {
                    drainedItems.Add(memberItem);
                    continue;
                }

                SelectAction selectAction = new SelectAction()
                {
                    Action = _combatService.GetActionFromSpell(party, memberItem.Member, memberItem.Spell, null, memberItem.UsableItem),
                    Member = memberItem.Member,
                    NextState = data.NextState,
                    ReturnState = data.ReturnState,
                };

                SelectSpellAction selectSpell = new SelectSpellAction()
                {
                    Action = selectAction,
                    Spell = memberItem.Spell,
                    PowerCost = 0,
                };

                char charCode = (char)('A' + m);
                string text = charCode + " " + memberItem.GetDescription();

                stateData.Actions.Add(new CrawlerStateAction(text,
                    (char)('A' + m), ECrawlerStates.OnSelectSpell, extraData: selectSpell));
            }

            if (drainedItems.Count > 0)
            {
                stateData.AddText("=================");
                stateData.AddText("Items on cooldown until next combat.");
            }
            foreach (MemberItemSpell drainedItem in drainedItems)
            {
                stateData.AddText(_textService.HighlightText(drainedItem.GetDescription(), TextColors.ColorRed));
            }

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, data.ReturnState, extraData: data));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
