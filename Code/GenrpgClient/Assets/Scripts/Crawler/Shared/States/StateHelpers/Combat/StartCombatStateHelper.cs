using OxDb.Client.Audio.ClientEvents;
using OxDb.Client.Audio.Constants;
using OxDb.Client.Crawler.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Combat
{
    public class StartCombatStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.StartCombat;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = null;

            PartyData party = _crawlerService.GetParty();

            InitialCombatState initialState = action.ExtraData as InitialCombatState;

            if (initialState == null)
            {
                if (party.InitialCombat != null)
                {
                    initialState = party.InitialCombat;
                }
                else
                {
                    initialState = new InitialCombatState();
                }
            }

            party.InitialCombat = initialState;

            if (await _combatService.StartCombat(_crawlerService.GetParty()))
            {
                _dispatcher.Dispatch(new PlaySound(CrawlerAudio.StartCombat, AudioConstants.NoVariance));
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }
            else
            {
                stateData = new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Failed to start combat.", };
            }

            await Task.CompletedTask;
            return stateData;
        }
    }
}


