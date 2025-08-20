using Assets.Scripts.BoardGame.Loading;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Services
{

    public interface ILoadBoardService : IInjectable
    {
        Awaitable LoadBoard(BoardData boardData, CancellationToken token);
    }


    public class LoadBoardService : ILoadBoardService
    {

        private IScreenService _screenService;
        private ILogService _logService;

        private OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep> _steps = new OrderedSetupDictionaryContainer<ELoadBoardSteps, ILoadBoardStep>();

        public async Awaitable LoadBoard(BoardData boardData, CancellationToken token)
        {
            _screenService.Open(ScreenNames.Loading);
            try
            {
                foreach (ILoadBoardStep step in _steps.OrderedItems())
                {
                    await step.Execute(boardData, token);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "LoadBoardMap");
            }

            await Awaitable.NextFrameAsync(token);
            _screenService.Close(ScreenNames.Loading);
        }
    }
}
