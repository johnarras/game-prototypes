using ClientEvents;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Units.Entities;
using System.Threading;

public class UnitFrameContainer : BaseBehaviour
{

    public UnitFrame Frame;

    private CancellationToken _token;

    private IPlayerManager _playerManager;

    public void Init(CancellationToken token)
    {
        _token = token;
        AddListener<SetMapPlayerEvent>(OnSetMapPlayer);
        AddListener<ExitMapEvent>(OnExitMap);
        _playerManager.TryGetUnit(out Unit unit);
        SetMapPlayer(unit);
    }

    private void OnExitMap(ExitMapEvent exitMap)
    {
        OnUpdatePlayer(exitMap.Ch);
        return;
    }

    private void OnSetMapPlayer(SetMapPlayerEvent sdata)
    {
        if (Frame == null)
        {
            return;
        }
        OnUpdatePlayer(sdata.Ch);
        return;
    }

    private void OnUpdatePlayer(Character ch)
    {
        if (ch != null)
        {
            SetMapPlayer(ch);
        }
        else
        {
            SetMapPlayer(null);
        }
    }

    private void SetMapPlayer(Unit unit)
    {
        if (unit == null)
        {
            _clientEntityService.SetActive(Frame, false);
        }
        else
        {
            _clientEntityService.SetActive(Frame, true);
            Frame.Init(unit);
        }
    }
}

