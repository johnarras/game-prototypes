using Genrpg.RequestServer.BoardGame.DiceRoll.Services;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.RollDice.WebApi;

namespace Genrpg.RequestServer.BoardGame.DiceRoll.RequestHandlers
{
    public class RollDiceRequestHandlers : BaseClientUserRequestHandler<RollDiceRequest>
    {
        IDiceRollService _diceRollService = null!;
        protected override async Task InnerHandleMessage(WebContext context, RollDiceRequest request, CancellationToken token)
        {
            await _diceRollService.RollDice(context, null);
        }
    }
}
