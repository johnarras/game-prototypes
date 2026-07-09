using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.DeleteMember
{
    public class DeleteConfirmHelper : BaseStateHelper
    {
        private IPartyService _partyService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.DeleteConfirm;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            stateData.Actions.Add(new CrawlerStateAction("Delete " + member.Name + "?\n\n", Key.None, ECrawlerStates.None, null, null,
                member.PortraitName));

            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.DeleteMember,
                delegate
                {
                    if (member.PartySlot > 0)
                    {
                        return;
                    }

                    PartyData party = _crawlerService.GetParty();

                    _partyService.DeletePartyMemberFromGuild(party, member);

                    _crawlerService.SaveGame();

                }));

            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.DeleteMember));

            await Task.CompletedTask;
            return stateData;
        }
    }
}


