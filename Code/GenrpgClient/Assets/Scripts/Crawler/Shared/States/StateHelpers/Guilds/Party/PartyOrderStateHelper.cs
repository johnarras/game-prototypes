using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Guilds.Party
{



    public class PartyOrderStateHelper : BaseCombatStateHelper
    {

        private IPartyService _partyService = null;

        class PartyArrangement
        {
            public List<PartyMember> NewOrder { get; set; } = new List<PartyMember>();
            public List<PartyMember> OldOrder { get; set; } = new List<PartyMember>();
        }


        public override ECrawlerStates Key => ECrawlerStates.PartyOrder;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            List<PartyMember> partyMembers = party.GetActiveParty();

            PartyArrangement arrangement = currentData.ExtraData as PartyArrangement;

            if (arrangement == null)
            {
                arrangement = new PartyArrangement() { OldOrder = partyMembers };
            }

            stateData.ExtraData = arrangement;

            stateData.Actions.Add(new CrawlerStateAction("New Party Arrangement:"));
            for (int i = 0; i < arrangement.NewOrder.Count; i++)
            {

                PartyMember member = arrangement.NewOrder[i];
                stateData.Actions.Add(new CrawlerStateAction(member.Name, CharCodes.None, ECrawlerStates.PartyOrder,
                () =>
                    {
                        arrangement.NewOrder.Remove(member);
                        arrangement.OldOrder.Add(member);
                    }, arrangement));
            }

            stateData.Actions.Add(new CrawlerStateAction("\nArrange These Party Members:"));


            _crawlerService.GetTopLevelState();
            for (int i = 0; i < arrangement.OldOrder.Count; i++)
            {
                PartyMember member = arrangement.OldOrder[i];
                stateData.Actions.Add(new CrawlerStateAction(member.Name, (char)('1' + i), ECrawlerStates.PartyOrder,
                    () =>
                    {
                        arrangement.OldOrder.Remove(member);
                        arrangement.NewOrder.Add(member);

                        if (arrangement.OldOrder.Count <= 1)
                        {
                            arrangement.NewOrder.AddRange(arrangement.OldOrder);
                            _partyService.RearrangePartySlots(party, arrangement.NewOrder);
                            _crawlerService.ChangeState(_crawlerService.GetPrevState(ECrawlerStates.GuildMain), token);
                        }

                    }, arrangement));
            }

            stateData.Actions.Add(new CrawlerStateAction("\nExit Without Saving", CharCodes.Escape, _crawlerService.GetPrevState(ECrawlerStates.GuildMain)));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
