using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.RemoveMember
{
    public class RemoveMemberHelper : BaseStateHelper
    {
        private IPartyService _partyService;
        public override ECrawlerStates Key => ECrawlerStates.RemoveMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            for (int m = 0; m < party.Members.Count; m++)
            {
                PartyMember member = party.Members[m];

                if (member.PartySlot == 0)
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(member.Name, CharCodes.None, ECrawlerStates.RemoveMember,
                    delegate
                    {
                        if (member.PartySlot < 1)
                        {
                            return;
                        }


                        _partyService.RemovePartyMember(party, member);
                        _statService.CalcPartyStats(party, true);
                        _crawlerService.SaveGame();

                    }, member));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain, null, null));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
