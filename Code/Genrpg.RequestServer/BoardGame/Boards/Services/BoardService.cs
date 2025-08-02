using Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Tiles.Settings;

namespace Genrpg.RequestServer.BoardGame.Boards.Services
{
    public interface IBoardService : IInjectable
    {
        ITileTypeHelper GetTileTypeHelper(long tileTypeId);

        List<long> GetTileTypesWithPrizes(WebContext context);
    }


    public class BoardService : IBoardService
    {
        private IGameData _gameData = null;
        private SetupDictionaryContainer<long, ITileTypeHelper> _tileTypeHelpers = new SetupDictionaryContainer<long, ITileTypeHelper>();

        public ITileTypeHelper GetTileTypeHelper(long tileTypeId)
        {
            if (_tileTypeHelpers.TryGetValue(tileTypeId, out ITileTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public List<long> GetTileTypesWithPrizes(WebContext context)
        {
            return _gameData.Get<TileTypeSettings>(context.user).GetData().Select(X => X.IdKey).ToList();
        }
    }
}
