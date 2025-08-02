using Genrpg.RequestServer.BoardGame.Helpers.BoardLoadHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Loading.Services
{
    public interface IBoardLoadService : IInjectable
    {
        Task AfterBoardLoad(WebContext context);
    }
    public class BoardLoadService : IBoardLoadService
    {
        private SetupDictionaryContainer<Type, IBoardLoadHelper> _boardLoadHelpers = new SetupDictionaryContainer<Type, IBoardLoadHelper>();
        public async Task AfterBoardLoad(WebContext context)
        {
            await Task.CompletedTask;
        }
    }
}
