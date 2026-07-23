
using OxDb.Client.Setup.Interfaces;
using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.Setup.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSetupService : SetupService
{
    public override async Task SetupGame(IGameState gs, object initObject, List<object> existingObjects, CancellationToken token)
    {

        try
        {
            await base.SetupGame(gs, initObject, existingObjects, token);

            foreach (IGameTokenService service in gs.loc.GetVals<IGameTokenService>())
            {
                service.SetGameToken(token);
            }
        }
        catch (Exception ee)
        {
            Debug.Log("Exception on init: " + ee.Message + " " + ee.StackTrace);
        }


    }
}


