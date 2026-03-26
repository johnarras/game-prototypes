using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Trader.CurrencySpend.Services;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Genrpg.RequestServer.Trader.CurrencySpend.RequestHandlers
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
