
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ClientLogService : ILogService
{

    const string LogPrefix = "UnityLog";
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private ClientConfig _config = null;
    private IDispatcher _dispatcher = null;
    public ClientLogService(ClientConfig config)
    {
        _config = config;
    }

    public async Task PrioritySetup(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public int SetupPriorityAscending() { return SetupPriorities.Logging; }


    public void Debug(string txt)
    {
        UnityEngine.Debug.Log(LogPrefix + "Log: " + txt);
    }

    public void Error(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(LogPrefix + "Error: " + txt);
    }

    public void Exception(Exception e, string txt)
    {
        if (e.GetType().IsAssignableFrom(typeof(TaskCanceledException)))
        {
            return;
        }
        _dispatcher.Dispatch(new ShowFloatingText(txt + " " + e.Message + " " + e.StackTrace, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(LogPrefix + "Exc: " + txt + " -- " + e.Message + " " + e.StackTrace);
    }


    public void Info(string txt)
    {
        UnityEngine.Debug.Log(LogPrefix + "Info: " + txt);
    }

    public void Message(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Message));
        UnityEngine.Debug.Log(LogPrefix + "Message: " + txt);
    }

    public void Warning(string txt)
    {
        UnityEngine.Debug.LogWarning(LogPrefix + "Warning: " + txt);
    }
}


