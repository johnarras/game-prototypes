using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Core.Interfaces;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Minimap.Services
{
    public interface IMinimapService : IInjectable, IClientResetCleanup
    {
        void SetTexture(Texture2D tex);
        Texture2D GetTexture();
    }

    public class MinimapService : IMinimapService
    {
        private IClientEntityService _clientEntityService = null;
        private IDispatcher _dispatcher = null;

        private Texture2D _minimapTexture = null;

        public Texture2D GetTexture()
        {
            return _minimapTexture;
        }

        public async Task OnReset(CancellationToken token)
        {
            if (_minimapTexture != null)
            {
                _clientEntityService.Destroy(_minimapTexture);
                _minimapTexture = null;
            }
            await Task.CompletedTask;
        }

        public void SetTexture(Texture2D tex)
        {
            if (_minimapTexture != null)
            {
                _clientEntityService.Destroy(_minimapTexture);
            }
            _minimapTexture = tex;
            _dispatcher.Dispatch(new SetMinimapTexture() { Texture = tex });
        }
    }
}


