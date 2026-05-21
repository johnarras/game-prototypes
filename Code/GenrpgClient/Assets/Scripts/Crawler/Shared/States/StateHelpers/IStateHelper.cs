using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers
{
    public interface IStateHelper : ISetupDictionaryItem<ECrawlerStates>
    {
        Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token);
        bool IsTopLevelState();
        long TriggerBuildingId();
        long TriggerDetailEntityTypeId();
        bool HideBigPanels();
        bool ShouldDispatchClickKeys();
    }
}


