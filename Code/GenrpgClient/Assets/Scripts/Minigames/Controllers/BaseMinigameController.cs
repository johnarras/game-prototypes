using OxDb.Client.Awaitables;
using OxDb.Client.Minigames.Services;
using OxDb.Client.Minigames.UI;
using OxDb.SharedGame.Minigames.Games.Settings;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Minigames.Controllers
{
    public abstract class BaseMinigameController : BaseBehaviour
    {

        public BaseMinigameUI UI;

        protected IAwaitableService _awaitableService = null;
        protected IClientMinigameService _clientMinigameService = null;

        protected abstract Awaitable OnSetDataAsync(CancellationToken token);

        private MinigameType _mtype = null;
        public virtual void SetData(MinigameType mtype)
        {
            _mtype = mtype;
            _awaitableService.ForgetAwaitable(OnSetDataAsync(GetToken()));


            UI.SetData(mtype);
        }
    }
}
