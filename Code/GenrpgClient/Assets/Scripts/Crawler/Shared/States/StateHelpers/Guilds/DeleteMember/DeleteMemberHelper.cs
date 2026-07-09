using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.DeleteMember
{
    public class DeleteMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.DeleteMember;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            for (int m = 0; m < party.InGuild.Count; m++)
            {
                PartyMember member = party.InGuild[m];

                stateData.Actions.Add(new CrawlerStateAction(member.Name, Key.None, ECrawlerStates.DeleteConfirm, null,
                    member, member.PortraitName));

            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.GuildMain, null, null));

            await Task.CompletedTask;

            return stateData;
        }
    }
}


