using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.SharedGame.Trader.CurrencySpend.Services;
using OxDb.SharedGame.Trader.CurrencySpend.WebApi;

namespace OxDb.RequestServer.Trader.CurrencySpend.RequestHandlers
{
    public class SpendCurrencyRequestHandler : BaseClientUserRequestHandler<SpendCurrencyRequest>
    {
        private ICurrencySpendService _spendService = null;
        protected override async Task InnerHandleMessage(WebContext context, SpendCurrencyRequest request, CancellationToken token)
        {
            context.AddResponse(await _spendService.SpendCurrency(context, request));

        }
    }
}
