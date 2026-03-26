
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Core;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ClientLogService : ILogService, IClientQuitCleanup
{

    const string LogPrefix = "UnityLog";
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    List<string> messages = new List<string>();

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

        UnityEngine.Debug.Log(GetFullLogText("Debug: ", txt));
    }

    public void Error(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(GetFullLogText("Error: ", txt));
    }

    public void Exception(Exception e, string txt)
    {
        if (e.GetType().IsAssignableFrom(typeof(TaskCanceledException)))
        {
            return;
        }
        _dispatcher.Dispatch(new ShowFloatingText(txt + " " + e.Message + " " + e.StackTrace, EFloatingTextArt.Error));
        UnityEngine.Debug.LogError(GetFullLogText("Exc: ", txt + " -- " + e.Message + " " + e.StackTrace));
    }


    public void Info(string txt)
    {
        UnityEngine.Debug.Log(GetFullLogText("Info: ", txt));
    }

    public void Message(string txt)
    {
        _dispatcher.Dispatch(new ShowFloatingText(txt, EFloatingTextArt.Message));
        UnityEngine.Debug.Log(GetFullLogText("Message: ", txt));
    }

    public void Warning(string txt)
    {
        UnityEngine.Debug.LogWarning(GetFullLogText("Warning: ", txt));
    }

    private string GetFullLogText(string textPrefix, string txt)
    {
        string newText = LogPrefix + textPrefix + txt;
        messages.Add(newText);
        return newText;
    }

    public void OnQuit()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "LogFile.txt");

        StringBuilder sb = new StringBuilder();

        foreach (string line in messages)
        {
            sb.AppendLine(line);
        }

        File.WriteAllText(fullPath, sb.ToString());
    }
}


