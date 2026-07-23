
using OxDb.Client.Assets.ObjectPools;
using OxDb.SharedGame.UI.Interfaces;
using System;
using System.Threading;

public class GButton : UnityEngine.UI.Button, IButton, IDestroyCallback
{
    public CancellationToken GetToken()
    {
        return destroyCancellationToken;
    }

    protected CancellationTokenRegistration _ctRegistration;

    public void SetDestroyCallback(Action action)
    {
        _ctRegistration.Dispose();

        if (action == null)
        {
            return;
        }

        _ctRegistration = destroyCancellationToken.Register(action);
    }

    protected override void OnDestroy()
    {
        _ctRegistration.Dispose();
    }
}

