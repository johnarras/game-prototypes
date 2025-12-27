using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.DeleteMember
{
    public class DeleteMemberHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.DeleteMember;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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


