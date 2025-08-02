using Genrpg.RequestServer.BoardGame.BoardGen;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.BoardGame.Upgrades.WebApi;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Upgrades.Services
{
    public interface IServerUpgradeBoardService : IInjectable
    {
        Task UpgradeTileType(WebContext context, long tileTypeId);
    }


    public class ServerUpgradeBoardService : IServerUpgradeBoardService
    {

        private IUpgradeBoardService _upgradeBoardService = null!;
        private IBoardGenService _boardGenService = null!;

        public async Task UpgradeTileType(WebContext context, long tileTypeId)
        {
            BoardData boardData = await context.GetAsync<BoardData>();
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            UpgradeBoardResponse response = new UpgradeBoardResponse() { TileTypeId = tileTypeId };

            if (!boardData.IsOwnBoard())
            {                
                response.Message = "You aren't on your own board!";
                context.Responses.AddResponse(response);
                return;
            }

            response.Costs = _upgradeBoardService.GetUpgradeCosts(context.user, userData, tileTypeId, boardData.GetCurrentUpgradeTier(tileTypeId)); ;

            if (!response.Costs.CanUpgradeNow)
            {
                context.Responses.AddResponse(response);
                return;
            }

            foreach (UpgradeReagent reagent in response.Costs.Reagents)
            {
                userData.Coins.Add(reagent.UserCoinTypeId, -reagent.RequiredQuantity);
            }

            boardData.TileTypeUpgradeTiers.Add(tileTypeId, 1);
            response.NewTier = boardData.GetCurrentUpgradeTier(tileTypeId);
            response.Success = true;
           
            context.Responses.AddResponse(response);

            await _boardGenService.TryLevelUpBoard(context, boardData, userData);
        }
    }
}
