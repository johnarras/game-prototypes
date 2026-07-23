using OxDb.Client;
using OxDb.Client.GameSettings.Services;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class EditorGameDataUtils
{

    [MenuItem("Tools/ClearEditorGameState")]
    public static void ClearGameState()
    {
        _gs = null;
    }

#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static IClientGameState _gs = null;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    public static IClientGameState GetEditorGameState(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _gs = null;
        }
        if (_gs != null)
        {
            return _gs;
        }
        _gs = Setup().GetAwaiter().GetResult();
        return _gs;
    }

    public static List<IIdName> GetEntityListForEntityTypeId(long entityTypeId)
    {
        _gs = GetEditorGameState();

        IEntityService entityService = _gs.loc.Get<IEntityService>();

        return entityService.GetChildList(null, entityTypeId);
    }

    private static async Awaitable<IClientGameState> Setup()
    {
        try
        {
            if (_gs != null)
            {
                return _gs;
            }
            CancellationTokenSource _cts = new CancellationTokenSource();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            GameObject initObject = GameObject.Find("InitClient");

            InitClient initClient = initObject.GetComponent<InitClient>();

            IClientGameState gs = await initClient.InitialSetup();

            IClientConfigContainer configContainer = gs.loc.Get<IClientConfigContainer>();

            IClientGameDataService _clientGameDataService = gs.loc.Get<IClientGameDataService>();
            await _clientGameDataService.EditorLoadCachedSettings(gs);

            return gs;
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception on editor game state: " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }
}


