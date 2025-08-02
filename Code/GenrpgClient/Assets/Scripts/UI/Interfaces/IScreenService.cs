using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Interfaces
{
    public interface IScreenService : IInitializable
    {
        void Open(long screenId, object data = null);
        void StringOpen(string screenName, object data = null);
        void Close(long screenId);
        void FinishClose(long screenId);

        void StartUpdates();

        ActiveScreen GetScreen(long screenId);

        ActiveScreen GetLayerScreen(ScreenLayers layerId);

        List<ActiveScreen> GetScreensNamed(long screenId);

        public ActiveScreen GetScreen(string screenName);

        List<ActiveScreen> GetAllScreens();

        void CloseAll(List<long> ignoreScreens = null);

        object GetDragParent();
        string GetSubdirectory(long screenId);
        string GetFullScreenNameFromId(long id);

        Task<IScreen> OpenAsync(long screenId, object data, CancellationToken token);
    }
}
