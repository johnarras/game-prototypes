using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI; // FIX

public abstract class BaseScreen : AnimatorBehaviour, IScreen
{
    public long ScreenId { get; set; }
    public string Subdirectory { get; set; }
    public float IntroTime;
    public float OutroTime;

    protected object _openData;

    private List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();

    protected IAwaitableService _awaitableService = null;
    protected IScreenService _screenService = null;
    protected IRealtimeNetworkService _networkService = null;

    // Called when screen first opens.
    protected abstract Task OnStartOpen(object data, CancellationToken token);


    public override void Init()
    {
        base.Init();
        AddUpdate(ScreenUpdate, UpdateTypes.Regular);
    }

    protected virtual void OnEnable()
    {
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
        {
            _raycasters.Insert(0, gr);
        }
    }

    protected virtual void OnDisable()
    {
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
        {
            _raycasters.Remove(gr);
        }
    }

    

    private string _analyticsName = null;
    public override string GetName()
    {
        if (string.IsNullOrEmpty(_analyticsName))
        {
            _analyticsName = StrUtils.ToSnakeCase(name);
        }
        return _analyticsName;
    }

    protected List<GraphicRaycaster> GetAllRaycasters()
    {
        return _raycasters;
    }

    public virtual async Task StartOpen(object data, CancellationToken token)
    {
        _openData = data;
        await OnStartOpen(_openData, GetToken());

        if (IntroTime > 0)
        {
            TriggerAnimation(AnimParams.Intro, IntroTime, OnFinishOpen, GetToken());
        }
        else
        {
            OnFinishOpen(token);
        }
    }

    // Called as the screen finishes opening.
    protected virtual void OnFinishOpen(CancellationToken token)
    {
    }

    protected virtual void ScreenUpdate()
    {

    }

    public virtual void OnInfoChanged()
    {

    }

    public virtual void OnReset()
    {

    }

    public virtual bool BlockMouse()
    {
        return true;
    }

    public virtual void ErrorClose(string txt)
    {
        if (!string.IsNullOrEmpty(txt))
        {
            _logService.Info("Error on close: " + txt);
        }

        StartClose();
    }


    public virtual void StartClose()
    {
        OnStartClose();
        if (OutroTime > 0)
        {
            TriggerAnimation(AnimParams.Outro, OutroTime, OnFinishClose, GetToken());
        }
        else
        {
            OnFinishClose(GetToken());
        }
    }

    // Called immediately on start close.
    protected virtual void OnStartClose()
    {

    }

    // Called after close animation ends.
    protected virtual void OnFinishClose(CancellationToken token)
    {
        _dispatcher.Dispatch(new FinishCloseScreen(ScreenId));
    }
}





