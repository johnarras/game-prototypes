using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Core;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minimap.Services
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


