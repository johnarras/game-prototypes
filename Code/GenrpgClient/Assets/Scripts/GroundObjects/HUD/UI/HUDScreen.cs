
using Assets.Scripts.Resets.ClientEvents;
using Assets.Scripts.UI;
using System.Threading;
using System.Threading.Tasks;
using UI;

public class HUDScreen : BaseScreen
{

    public ZoneUI _zoneUI;


    public MinimapUI _minimap;


    public UnitFrameContainer _unitFrame;


    public NetworkStatusUI _networkStatus;


    public ActionBars _actionBars;


    public ChatWindow _chatWindow;

    public GButton ResetButton;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        _zoneUI?.Init(token);
        _minimap?.Init(token);
        _unitFrame?.Init(token);
        _networkStatus?.Init(token);
        _actionBars?.Init(token);

        _uiService.SetButton(ResetButton, name, OnClickReset);

        await Task.CompletedTask;
    }

    private void OnClickReset()
    {
        _dispatcher.Dispatch(new FullResetGame());
    }
}


