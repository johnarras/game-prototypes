
using OxDb.SharedGame.Errors.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ErrorMessageHandler : BaseClientMapMessageHandler<ErrorMessage>
{
    protected override async Awaitable InnerProcess(ErrorMessage msg, CancellationToken token)
    {
        await Task.CompletedTask;
        _logService.Error(msg.ErrorText);
    }
}


