
using Assets.Scripts.UI.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Errors
{
    public class ErrorStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.Error;



        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.ClearBGImage = true;

            PartyData party = _crawlerService.GetParty();

            _combatService.EndCombat(party);

            stateData.AddText("An error occurred.\nReturning you to the main map.");

            if (currentData.ExtraData != null && !string.IsNullOrEmpty(currentData.ExtraData.ToString()))
            {
                stateData.AddText(_textService.HighlightText("\n" + currentData.ExtraData.ToString() + "\n", TextColors.ColorRed));
            }

            AddSpaceAction(stateData);

            await Task.CompletedTask;
            return stateData;
        }
    }
}


