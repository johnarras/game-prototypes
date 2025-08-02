using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.AddMember
{
    public class AddMemberHelper : BaseStateHelper
    {
        private IPartyService _partyService;
        public override ECrawlerStates Key => ECrawlerStates.AddMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.AddText("Your max party size is " + _partyService.GetMaxPartySize(party));
            for (int m = 0; m < party.Members.Count; m++)
            {
                PartyMember member = party.Members[m];

                if (member.PartySlot > 0)
                {
                    continue;
                }
                    stateData.Actions.Add(new CrawlerStateAction(member.Name, CharCodes.None, ECrawlerStates.AddMember,
                    delegate
                    {
                        if (member.PartySlot > 0)
                        {
                            return;
                        }

                        party = _crawlerService.GetParty();

                        _partyService.AddPartyMember(party, member);
                        _statService.CalcPartyStats(party, true);
                        _crawlerService.SaveGame();


                    }, member, member.PortraitName));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
