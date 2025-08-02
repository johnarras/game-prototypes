using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Lockouts.Constants;
using Assets.Scripts.Lockouts.Services;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.RollDice.WebApi;
using System.Threading;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class RollDiceResponseHandler : BaseClientWebResponseHandler<RollDiceResponse>
    {
        private IBoardGameController _boardGameController;
        private ILockoutManager _lockoutManager;
        protected override void InnerProcess(RollDiceResponse result, CancellationToken token)
        {
            _lockoutManager.RemoveLock(LockoutTypes.RollDice, RollDiceLocks.SendRequest);
            _boardGameController.ShowDiceRoll(result);
        }
    }
}
