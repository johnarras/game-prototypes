using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.BoardGame.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.Upgrades.WebApi
{
    public class NextBoardDataResponseHandler : BaseClientWebResponseHandler<NextBoardData>
    {
        private IBoardGameController _controller = null!;
        protected override void InnerProcess(NextBoardData response, CancellationToken token)
        {
            _gs.ch.Set(response.NextBoard);
            _controller.LoadCurrentBoard();
        }
    }
}
