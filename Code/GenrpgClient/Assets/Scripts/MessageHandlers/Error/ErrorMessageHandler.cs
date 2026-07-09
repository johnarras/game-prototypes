
using OxDb.SharedGame.Errors.Messages;
using System.Threading;
using System.Threading.Tasks;

public class ErrorMessageHandler : BaseClientMapMessageHandler<ErrorMessage>
{
    protected override async ValueTask InnerProcess(ErrorMessage msg, CancellationToken token)
    {
        await Task.CompletedTask;
        _logService.Error(msg.ErrorText);
    }
}


