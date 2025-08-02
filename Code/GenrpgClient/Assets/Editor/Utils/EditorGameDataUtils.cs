using Assets.Scripts.GameSettings.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class EditorGameDataUtils
{

    [MenuItem("Tools/ClearEditorGameState")]
    static void ExecuteDev()
    {
        _gs = null;
    }

    private static IClientGameState _gs = null;

    public static IClientGameState GetEditorGameState()
    {
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

            IClientGameState gs = await initClient.InitialSetup(false);

            IClientConfigContainer configContainer = gs.loc.Get<IClientConfigContainer>();

            IClientGameDataService _clientGameDataService = gs.loc.Get<IClientGameDataService>();
            await _clientGameDataService.LoadCachedSettings(gs);
            return gs;
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception on editor game state: " + ex.Message + " " + ex.StackTrace);
        }
        return null;
    }
}
