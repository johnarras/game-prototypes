using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LoadSave.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.Services
{
    public interface ICrawlerService : IInitializable
    {
        CancellationToken GetToken();
        void ChangeState(ECrawlerStates state, CancellationToken token, object extraData = null, ECrawlerStates returnState = ECrawlerStates.None);
        void ChangeState(CrawlerStateData currentState, CrawlerStateAction nextStateAction, CancellationToken token);
        PartyData GetParty();
        Task SaveGame();
        PartyData LoadParty(long slotId = LoadSaveConstants.MinSlot);
        void ClearAllStates();
        bool ContinueGame();
        CrawlerStateData PopState();
        CrawlerStateData GetTopLevelState();
        ECrawlerStates GetState();
        void NewGame();
        void ClearSpeedup();
        List<IStateHelper> GetAllStateHelpers();
        void UpdateInputs(CancellationToken token);
        long GetCrawlerScreenId();
        ECrawlerStates GetPrevState(ECrawlerStates tryPrevState = ECrawlerStates.None);
    }
}
