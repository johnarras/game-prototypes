
using Microsoft.Extensions.Logging;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Setup.Constants;

namespace OxDb.ServerCore.Logalytics.Services
{
    public class ServerLogService : ILogService
    {

        private ILogger _logger = null;

        public async Task PrioritySetup(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public ServerLogService(ILogger logger)
        {
            _logger = logger;
        }

        public int SetupPriorityAscending() { return SetupPriorities.Logging; }

        public void Info(string txt)
        {
            _logger.LogInformation(txt);
        }
        public void Warning(string txt)
        {
            _logger.LogWarning(txt);
        }
        public void Debug(string txt)
        {
            _logger.LogDebug(txt);
        }

        public void Error(string txt)
        {
            _logger.LogError(txt);
        }

        public void Exception(Exception e, string txt)
        {
            _logger.LogCritical(e.Message + " -- " + txt + " -- " + e.StackTrace);
        }
    }
}


