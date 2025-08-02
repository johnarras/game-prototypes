using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Markers.Services;
using Assets.Scripts.BoardGame.Players;
using Assets.Scripts.BoardGame.Tiles;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Loading.Steps
{
    public class LoadPlayer : BaseLoadBoardStep
    {
        public override ELoadBoardSteps Key => ELoadBoardSteps.LoadPlayer;

        private IClientMarkerService _clientMarkerService = null;
        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            _clientMarkerService.ClientSetMarkerId(userData.Vars.Get(UserVars.MarkerId), userData.Vars.Get(UserVars.MarkerTier));
            await Task.CompletedTask;
        }
    }
}
