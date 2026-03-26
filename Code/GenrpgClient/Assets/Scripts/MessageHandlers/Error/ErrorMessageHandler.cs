
using Genrpg.Shared.Errors.Messages;
using System.Threading;
using UnityEngine;

public class ErrorMessageHandler : BaseClientMapMessageHandler<ErrorMessage>
{
    protected override async Awaitable InnerProcess(ErrorMessage msg, CancellationToken token)
    {
        _logService.Error(msg.ErrorText);
    }
}


