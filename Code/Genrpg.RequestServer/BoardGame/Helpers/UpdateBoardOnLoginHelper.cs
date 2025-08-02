using Genrpg.RequestServer.BoardGame.BoardGen;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.LoadUpdateHelpers;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.RequestServer.BoardGame.Helpers
{
    public class UpdateBoardOnLoginHelper : IUserLoadUpdater
    {
        protected IBoardGenService _boardGenService = null!;
        protected IRepositoryService _repoService = null!;

        public int Order => 0;
        public Type Key => GetType();

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            BoardData boardData = await context.GetAsync<BoardData>();

            if (!boardData.IsValid())
            {
                boardData = await _boardGenService.GenerateBoard(context);
                await _repoService.Save(boardData);

                BoardData existingData = (BoardData)unitData.FirstOrDefault(x => x is BoardData boardData2);
                if (existingData != null)
                {
                    unitData.Remove(existingData);
                }
                unitData.Add(boardData);
            }

        }
    }
}
