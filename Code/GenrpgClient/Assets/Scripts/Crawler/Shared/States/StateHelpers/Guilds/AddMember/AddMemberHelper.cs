using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.AddMember
{
    public class AddMemberHelper : BaseStateHelper
    {
        private IPartyService _partyService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.AddMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            stateData.AddText("Your max party size is " + _partyService.GetMaxPartySize(party));
            for (int m = 0; m < party.InGuild.Count; m++)
            {
                PartyMember member = party.InGuild[m];

                stateData.Actions.Add(new CrawlerStateAction(member.Name, Key.None, ECrawlerStates.AddMember,
                delegate
                {
                    party = _crawlerService.GetParty();
                    _partyService.AddActivePartyMember(party, member);
                    _statService.CalcPartyStats(party, true);
                    _crawlerService.SaveGame();


                }, member, member.PortraitName));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


