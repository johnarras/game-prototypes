using OxDb.Client.ClientEvents.UI;
using OxDb.SharedGame.Minigames.Games.Settings;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Minigames.Lobby
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

            minigames = minigames.OrderBy(x => x.MinLevel).ToList();


            foreach (MinigameType mtype in minigames)
            {
                MinigameLobbyIcon icon = _clientEntityService.FullInstantiate<MinigameLobbyIcon>(IconPrefab);

                _clientEntityService.AddToParent(icon, IconParent);

                icon.SetData(mtype, this);
            }
            await Task.CompletedTask;
        }
    }
}
