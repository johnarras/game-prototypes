using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.RemoveMember
{
    public class RemoveMemberHelper : BaseStateHelper
    {
        private IPartyService _partyService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.RemoveMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            for (int m = 0; m < party.ActiveParty.Count; m++)
            {
                PartyMember member = party.ActiveParty[m];

                stateData.Actions.Add(new CrawlerStateAction(member.Name, Key.None, ECrawlerStates.RemoveMember,
                    delegate
                    {
                        _partyService.RemoveActivePartyMember(party, member);
                        _statService.CalcPartyStats(party, true);
                        _crawlerService.SaveGame();

                    }, member));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.GuildMain, null, null));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


