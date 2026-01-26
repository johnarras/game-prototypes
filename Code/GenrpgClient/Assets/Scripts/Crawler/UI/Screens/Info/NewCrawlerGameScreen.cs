using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.Screens.Info
{
    public class NewCrawlerGameScreen : BaseScreen, IPointerDownHandler
    {

        private IInputService _inputService = null;
        public GText Text;

        public int FramesBetweenNewLetter = 5;

        private int _currentLetterFrame = 0;
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            Text.maxVisibleCharacters = 0;
            await Task.CompletedTask;
        }

        protected override void ScreenUpdate()
        {
            if (++_currentLetterFrame >= FramesBetweenNewLetter)
            {
                _currentLetterFrame = 0;

                Text.maxVisibleCharacters++;
            }

            if (_inputService.ContinueKeyIsDown())
            {
                CloseScreenAction();
                return;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            CloseScreenAction();
        }

        private void CloseScreenAction()
        {
            StartClose();
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerNewGameOptions));

        }

    }
}


