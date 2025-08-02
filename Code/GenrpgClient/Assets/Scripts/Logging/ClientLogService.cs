
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ClientLogService : ILogService
{

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private ClientConfig _config = null;
    private IDispatcher _dispatcher = null;
    private ITextSerializer _serializer = null;
    public ClientLogService(ClientConfig config, ITextSerializer serializer)
    {
        _config = config;
        _serializer = serializer;
    }

    public async Task PrioritySetup(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public int SetupPriorityAscending() { return SetupPriorities.Logging; }


    public void Debug(string txt)
    {
        UnityEngine.Debug.Log(txt);
    }

    public void Error(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(txt);
    }

    public void Exception(Exception e, string txt)
    {
        if (e.GetType().IsAssignableFrom(typeof(TaskCanceledException)))
        {
            return;
        }
        _dispatcher.Dispatch(new ShowFloatingText(txt + " " + e.Message + " " + e.StackTrace, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(txt + " " + e.GetType().Name + " -- " + e.Message + " " + e.StackTrace);
    }

    public void Info(string txt)
    {
        UnityEngine.Debug.Log(txt);
    }

    public void Message(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Message));
        UnityEngine.Debug.Log(txt);
    }

    public void Warning(string txt)
    {
        UnityEngine.Debug.LogWarning(txt);
    }
}
