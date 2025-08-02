using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnUpdateGameSettingsMessageHandler : BaseClientMapMessageHandler<UpdateGameSettings>
    {
        private IClientWebService _webNetworkService = null;

        protected override void InnerProcess(UpdateGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendClientUserWebRequest(new UpdateGameSettingsRequest() { CharId = _gs.ch.Id }, token);
            _webNetworkService.SendClientUserWebRequest(new RefreshStoresRequest() { CharId = _gs.ch.Id }, token);
        }
    }
}
