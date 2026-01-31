using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Trader.MinigameTypes.Settings;
using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minigames.Lobby
{
    public class MinigameLobbyScreen : BaseScreen
    {

        public GameObject IconParent;

        public MinigameLobbyIcon IconPrefab;
      
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            _dispatcher.Dispatch(new CloseAllScreens() { KeepOpenScreens = new List<long>() { ScreenNames.MinigameLobby } });


            _awaitableService.ForgetAwaitable(ShowMinigamesAsync());
            await Task.CompletedTask;

        }

        private async Awaitable ShowMinigamesAsync()
        {
            _clientEntityService.DestroyAllChildren(IconParent);

            List<MinigameType> minigames = _gameData.Get<MinigameTypeSettings>(_gs.ch).GetData().ToList();

            minigames = minigames.OrderBy(x=>x.MinLevel).ToList();  


            foreach (MinigameType mtype in minigames)
            {
                MinigameLobbyIcon icon = _clientEntityService.FullInstantiate<MinigameLobbyIcon>(IconPrefab);

                _clientEntityService.AddToParent(icon, IconParent);

                icon.InitData(mtype, this);
            }
            await Task.CompletedTask;
        }
    }
}
