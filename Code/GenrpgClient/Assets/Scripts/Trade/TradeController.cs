using Assets.Scripts.ClientEvents.UI;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trade
{
    public interface ITradeController : IInitializable
    {
        void HandleOnStartTrade(OnStartTrade onStartTrade);
    }

    public class TradeController : ITradeController
    {

        private IDispatcher _dispatcher = null;

        private CancellationToken _token;
        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<OnStartTrade>(HandleOnStartTrade, _token);
            await Task.CompletedTask;
        }

        public void HandleOnStartTrade(OnStartTrade onStartTrade)
        {
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Trade, onStartTrade));
        }
    }
}


