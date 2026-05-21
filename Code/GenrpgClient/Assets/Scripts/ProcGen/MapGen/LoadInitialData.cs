using OxDb.SharedGame.Players.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using UnityEngine;


public class LoadInitialData : BaseZoneGenerator
{
    protected IScreenService _screenService = null;
    private IRealtimeNetworkService _networkService = null;
    private IPlayerManager _playerManager;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        _awaitableService.ForgetAwaitable(LoadInitialMapData(token));
    }

    public async Awaitable LoadInitialMapData(CancellationToken token)
    {
        _md.HaveSetHeights = true;
        _md.HaveSetAlphaSplats = true;

        float delaySec = 1.0f;

        _terrainManager.SetFastLoading();

        await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);

        while (_terrainManager.AddingPatches())
        {
            await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);
        }

        Unit unit = null;
        while (!_playerManager.TryGetUnit(out unit))
        {
            await Awaitable.WaitForSecondsAsync(delaySec, cancellationToken: token);
        }

        GameObject go = _playerManager.GetPlayerGameObject();
        if (unit != null && go != null)
        {
            float height = 0;

            do
            {
                height = _terrainManager.SampleHeight(unit.X, unit.Z);
                go.transform.position = new Vector3(unit.X, height, unit.Z);
                go.transform.eulerAngles = new Vector3(0, unit.Rot, 0);
                if (height == 0)
                {
                    await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
                }
            }
            while (height == 0);
        }
        _networkService.SendMapMessage(new AddPlayer()
        {
            GameUserId = _gs.GameUserId,
            SessionId = _gs.SessionState.SessionId,
            CharacterId = _gs.ch.Id,
        });

        PlayerController controller = go.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.StartUpdates();
        }

        _logService.Debug("LOADINTOMAP START " + _gs.GameUserId);

    }



}


